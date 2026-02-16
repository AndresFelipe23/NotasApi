using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotasApi.DTOs.Notas;
using NotasApi.Helpers;
using NotasApi.Repositories;

namespace NotasApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotasController : ControllerBase
{
    private readonly INotaRepository _notaRepository;

    public NotasController(INotaRepository notaRepository)
    {
        _notaRepository = notaRepository;
    }

    [HttpGet]
    public async Task<ActionResult> ObtenerNotas([FromQuery] Guid? carpetaId = null)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var notas = await _notaRepository.ObtenerResumenPorCarpetaAsync(usuarioId, carpetaId);
            return Ok(notas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener las notas", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> ObtenerNotaPorId(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var nota = await _notaRepository.ObtenerPorIdAsync(id, usuarioId);
            
            if (nota == null)
                return NotFound(new { message = "Nota no encontrada" });

            return Ok(nota);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener la nota", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CrearNota([FromBody] CrearNotaRequest request)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var notaId = await _notaRepository.CrearAsync(
                usuarioId,
                request.CarpetaId,
                request.Titulo,
                request.Resumen,
                request.Icono,
                request.ContenidoBloques
            );

            return CreatedAtAction(nameof(ObtenerNotaPorId), new { id = notaId }, new { id = notaId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear la nota", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> ActualizarNota(Guid id, [FromBody] ActualizarNotaRequest request)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _notaRepository.ActualizarAsync(
                id,
                usuarioId,
                request.Titulo,
                request.Resumen,
                request.Icono,
                request.ImagenPortadaUrl,
                request.ContenidoBloques
            );

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar la nota", error = ex.Message });
        }
    }

    [HttpPut("{id}/mover")]
    public async Task<ActionResult> MoverNota(Guid id, [FromBody] MoverNotaRequest request)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _notaRepository.MoverACarpetaAsync(id, usuarioId, request.CarpetaId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al mover la nota", error = ex.Message });
        }
    }

    [HttpPut("{id}/favorito")]
    public async Task<ActionResult> AlternarFavorito(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _notaRepository.AlternarFavoritoAsync(id, usuarioId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al alternar favorito", error = ex.Message });
        }
    }

    [HttpPut("{id}/archivar")]
    public async Task<ActionResult> ArchivarNota(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _notaRepository.ArchivarAsync(id, usuarioId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al archivar la nota", error = ex.Message });
        }
    }
}

public class MoverNotaRequest
{
    public Guid? CarpetaId { get; set; }
}
