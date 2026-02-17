using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotasApi.DTOs.Tareas;
using NotasApi.Helpers;
using NotasApi.Repositories;

namespace NotasApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TareasController : ControllerBase
{
    private readonly ITareaRepository _tareaRepository;

    public TareasController(ITareaRepository tareaRepository)
    {
        _tareaRepository = tareaRepository;
    }

    [HttpGet("pendientes")]
    public async Task<ActionResult> ObtenerPendientes()
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var tareas = await _tareaRepository.ObtenerPendientesAsync(usuarioId);
            return Ok(tareas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener las tareas pendientes", error = ex.Message });
        }
    }

    [HttpGet("completadas")]
    public async Task<ActionResult> ObtenerCompletadas()
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var tareas = await _tareaRepository.ObtenerCompletadasAsync(usuarioId);
            return Ok(tareas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener las tareas completadas", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CrearTarea([FromBody] CrearTareaRequest request)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var tareaId = await _tareaRepository.CrearAsync(
                usuarioId,
                request.Descripcion,
                request.NotaVinculadaId,
                request.Prioridad,
                request.FechaVencimiento
            );

            return CreatedAtAction(nameof(ObtenerPendientes), new { id = tareaId }, new { id = tareaId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear la tarea", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> ActualizarTarea(Guid id, [FromBody] ActualizarTareaRequest request)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _tareaRepository.ActualizarAsync(
                id,
                usuarioId,
                request.Descripcion,
                request.Prioridad,
                request.FechaVencimiento
            );

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar la tarea", error = ex.Message });
        }
    }

    [HttpPut("{id}/completar")]
    public async Task<ActionResult> AlternarEstado(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _tareaRepository.AlternarEstadoAsync(id, usuarioId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al cambiar el estado de la tarea", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarTarea(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _tareaRepository.EliminarAsync(id, usuarioId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar la tarea", error = ex.Message });
        }
    }
}
