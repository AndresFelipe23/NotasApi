using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NotasApi.Helpers;
using NotasApi.Services;

namespace NotasApi.Controllers;

[ApiController]
[Route("api/integrations/google")]
public class IntegracionesGoogleController : ControllerBase
{
    private readonly IGoogleTasksService _googleTasksService;

    public IntegracionesGoogleController(IGoogleTasksService googleTasksService)
    {
        _googleTasksService = googleTasksService;
    }

    /// <summary>
    /// Obtiene la URL para iniciar el flujo OAuth de Google Tasks. Requiere usuario autenticado.
    /// </summary>
    [HttpGet("auth-url")]
    [Authorize]
    public ActionResult GetAuthUrl()
    {
        var usuarioId = User.GetUsuarioId();
        var url = _googleTasksService.GetAuthorizationUrl(usuarioId);
        return Ok(new { authUrl = url });
    }

    /// <summary>
    /// Callback de Google OAuth. Google redirige aquí después de que el usuario autoriza.
    /// No requiere JWT (redirección desde Google).
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
    {
        var frontendBase = Request.HttpContext.RequestServices.GetRequiredService<IConfiguration>()["Google:FrontendBaseUrl"] ?? "https://anota.click";
        var frontendCallback = $"{frontendBase}/tareas?google=error";

        if (!string.IsNullOrEmpty(error))
        {
            var errorMsg = Uri.EscapeDataString(error == "access_denied" ? "Autorización cancelada" : error);
            return Redirect($"{frontendBase}/tareas?google=error&message={errorMsg}");
        }

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            return Redirect($"{frontendBase}/tareas?google=error&message=Parametros+invalidos");
        }

        Guid usuarioId;
        try
        {
            usuarioId = Guid.Parse(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(state)));
        }
        catch
        {
            return Redirect($"{frontendBase}/tareas?google=error&message=Estado+invalido");
        }

        try
        {
            var (success, errorMessage) = await _googleTasksService.ExchangeCodeForTokensAsync(code, state, usuarioId);

            if (success)
                return Redirect($"{frontendBase}/tareas?google=success");
            else
            {
                // Log del error para debugging
                Console.WriteLine($"Error en callback de Google: {errorMessage}");
                return Redirect($"{frontendBase}/tareas?google=error&message=" + Uri.EscapeDataString(errorMessage ?? "Error desconocido"));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Excepción en callback de Google: {ex.Message}\n{ex.StackTrace}");
            return Redirect($"{frontendBase}/tareas?google=error&message=" + Uri.EscapeDataString($"Error inesperado: {ex.Message}"));
        }
    }

    /// <summary>
    /// Verifica si el usuario tiene Google Tasks conectado.
    /// </summary>
    [HttpGet("status")]
    [Authorize]
    public async Task<ActionResult> GetStatus()
    {
        var usuarioId = User.GetUsuarioId();
        var (connected, email) = await _googleTasksService.GetConnectionStatusAsync(usuarioId);
        return Ok(new { connected, email });
    }

    /// <summary>
    /// Desconecta la integración de Google Tasks.
    /// </summary>
    [HttpDelete("disconnect")]
    [Authorize]
    public async Task<ActionResult> Disconnect()
    {
        var usuarioId = User.GetUsuarioId();
        await _googleTasksService.DisconnectAsync(usuarioId);
        return NoContent();
    }

    /// <summary>
    /// Obtiene las listas de tareas de Google del usuario.
    /// </summary>
    [HttpGet("task-lists")]
    [Authorize]
    public async Task<ActionResult> GetTaskLists()
    {
        var usuarioId = User.GetUsuarioId();
        var lists = await _googleTasksService.GetTaskListsAsync(usuarioId);
        return Ok(lists);
    }

    /// <summary>
    /// Obtiene las tareas de Google del usuario. Opcionalmente filtrar por taskListId.
    /// </summary>
    [HttpGet("tasks")]
    [Authorize]
    public async Task<ActionResult> GetTasks([FromQuery] string? taskListId = null)
    {
        var usuarioId = User.GetUsuarioId();
        var tasks = await _googleTasksService.GetTasksAsync(usuarioId, taskListId);
        return Ok(tasks);
    }

    /// <summary>
    /// Marca una tarea de Google como completada o pendiente.
    /// </summary>
    [HttpPut("tasks/{taskListId}/{taskId}/completar")]
    [Authorize]
    public async Task<ActionResult> CompleteTask(string taskListId, string taskId, [FromQuery] bool completada = true)
    {
        var usuarioId = User.GetUsuarioId();
        var success = await _googleTasksService.CompleteTaskAsync(usuarioId, taskListId, taskId, completada);
        if (!success)
            return BadRequest(new { message = "No se pudo actualizar la tarea de Google" });
        return NoContent();
    }

    /// <summary>
    /// Actualiza una tarea de Google (título y fecha de vencimiento).
    /// </summary>
    [HttpPut("tasks/{taskListId}/{taskId}")]
    [Authorize]
    public async Task<ActionResult> UpdateTask(string taskListId, string taskId, [FromBody] UpdateGoogleTaskRequest request)
    {
        var usuarioId = User.GetUsuarioId();
        var success = await _googleTasksService.UpdateTaskAsync(usuarioId, taskListId, taskId, request.Title, request.Due);
        if (!success)
            return BadRequest(new { message = "No se pudo actualizar la tarea de Google" });
        return NoContent();
    }

    /// <summary>
    /// Elimina una tarea de Google.
    /// </summary>
    [HttpDelete("tasks/{taskListId}/{taskId}")]
    [Authorize]
    public async Task<ActionResult> DeleteTask(string taskListId, string taskId)
    {
        var usuarioId = User.GetUsuarioId();
        var success = await _googleTasksService.DeleteTaskAsync(usuarioId, taskListId, taskId);
        if (!success)
            return BadRequest(new { message = "No se pudo eliminar la tarea de Google" });
        return NoContent();
    }
}

public class UpdateGoogleTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Due { get; set; }
}
