using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotasApi.DTOs.NotasRapidas;
using NotasApi.Helpers;
using NotasApi.Repositories;
using System.Diagnostics;

namespace NotasApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotasRapidasController : ControllerBase
{
    private readonly INotaRapidaRepository _notaRapidaRepository;
    private readonly ILogger<NotasRapidasController> _logger;

    public NotasRapidasController(INotaRapidaRepository notaRapidaRepository, ILogger<NotasRapidasController> logger)
    {
        _notaRapidaRepository = notaRapidaRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> ObtenerTodas()
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var usuarioId = User.GetUsuarioId();
            _logger.LogInformation("Obteniendo notas rápidas para usuario {UsuarioId}", usuarioId);

            var notasRapidas = await _notaRapidaRepository.ObtenerTodasAsync(usuarioId);

            sw.Stop();
            _logger.LogInformation("Notas rápidas obtenidas en {Elapsed}ms para usuario {UsuarioId}", sw.ElapsedMilliseconds, usuarioId);

            return Ok(notasRapidas);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "Error al obtener notas rápidas después de {Elapsed}ms", sw.ElapsedMilliseconds);
            return StatusCode(500, new {
                message = "Error al obtener las notas rápidas",
                error = ex.Message,
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CrearNotaRapida([FromBody] CrearNotaRapidaRequest request)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var notaRapidaId = await _notaRapidaRepository.CrearAsync(
                usuarioId,
                request.Contenido,
                request.ColorHex
            );

            return CreatedAtAction(nameof(ObtenerTodas), new { id = notaRapidaId }, new { id = notaRapidaId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear la nota rápida", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> ActualizarNotaRapida(Guid id, [FromBody] ActualizarNotaRapidaRequest request)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _notaRapidaRepository.ActualizarAsync(id, usuarioId, request.Contenido, request.ColorHex);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar la nota rápida", error = ex.Message });
        }
    }

    [HttpPut("{id}/archivar")]
    public async Task<ActionResult> ArchivarNotaRapida(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _notaRapidaRepository.ArchivarAsync(id, usuarioId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al archivar la nota rápida", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarNotaRapida(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _notaRapidaRepository.EliminarAsync(id, usuarioId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar la nota rápida", error = ex.Message });
        }
    }

    [HttpPost("{id}/convertir")]
    public async Task<ActionResult> ConvertirANota(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var notaId = await _notaRapidaRepository.ConvertirANotaAsync(usuarioId, id);
            return Ok(new { notaId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al convertir la nota rápida", error = ex.Message });
        }
    }
}
