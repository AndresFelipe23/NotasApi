using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotasApi.DTOs.Pizarras;
using NotasApi.Helpers;
using NotasApi.Repositories;

namespace NotasApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PizarrasController : ControllerBase
{
    private readonly IPizarraRepository _pizarraRepository;

    public PizarrasController(IPizarraRepository pizarraRepository)
    {
        _pizarraRepository = pizarraRepository;
    }

    [HttpGet]
    public async Task<ActionResult> ObtenerTodas([FromQuery] bool incluirArchivadas = false, [FromQuery] Guid? notaId = null)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();

            if (notaId.HasValue)
            {
                var porNota = await _pizarraRepository.ObtenerPorNotaAsync(usuarioId, notaId.Value);
                return Ok(porNota);
            }

            var pizarras = await _pizarraRepository.ObtenerTodasAsync(usuarioId, incluirArchivadas);
            return Ok(pizarras);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener las pizarras", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> ObtenerPorId(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var pizarra = await _pizarraRepository.ObtenerPorIdAsync(id, usuarioId);
            if (pizarra is null)
            {
                return NotFound();
            }
            return Ok(pizarra);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener la pizarra", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> Crear([FromBody] CrearPizarraRequest request)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var id = await _pizarraRepository.CrearAsync(usuarioId, request.Titulo, request.Descripcion, request.SceneJson, request.NotaId);
            return CreatedAtAction(nameof(ObtenerPorId), new { id }, new { id });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear la pizarra", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Actualizar(Guid id, [FromBody] ActualizarPizarraRequest request)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _pizarraRepository.ActualizarAsync(id, usuarioId, request.Titulo, request.Descripcion, request.SceneJson, request.NotaId, request.EsArchivada);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar la pizarra", error = ex.Message });
        }
    }

    [HttpPut("{id}/archivar")]
    public async Task<ActionResult> Archivar(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _pizarraRepository.ArchivarAsync(id, usuarioId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al archivar la pizarra", error = ex.Message });
        }
    }

    [HttpPut("{id}/recuperar")]
    public async Task<ActionResult> Recuperar(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _pizarraRepository.RecuperarAsync(id, usuarioId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al recuperar la pizarra", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Eliminar(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _pizarraRepository.EliminarAsync(id, usuarioId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar la pizarra", error = ex.Message });
        }
    }
}

