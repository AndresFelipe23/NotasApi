using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotasApi.DTOs.Etiquetas;
using NotasApi.Helpers;
using NotasApi.Repositories;

namespace NotasApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EtiquetasController : ControllerBase
{
    private readonly IEtiquetaRepository _etiquetaRepository;

    public EtiquetasController(IEtiquetaRepository etiquetaRepository)
    {
        _etiquetaRepository = etiquetaRepository;
    }

    [HttpGet]
    public async Task<ActionResult> ObtenerTodas()
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var etiquetas = await _etiquetaRepository.ObtenerPorUsuarioAsync(usuarioId);
            return Ok(etiquetas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener etiquetas", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> Crear([FromBody] CrearEtiquetaRequest request)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var id = await _etiquetaRepository.CrearAsync(usuarioId, request.Nombre, request.ColorHex);
            return CreatedAtAction(nameof(ObtenerTodas), new { id }, new { id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear etiqueta", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Actualizar(Guid id, [FromBody] ActualizarEtiquetaRequest request)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _etiquetaRepository.ActualizarAsync(id, usuarioId, request.Nombre, request.ColorHex);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar etiqueta", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Eliminar(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _etiquetaRepository.EliminarAsync(id, usuarioId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar etiqueta", error = ex.Message });
        }
    }
}
