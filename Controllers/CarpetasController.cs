using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotasApi.DTOs.Carpetas;
using NotasApi.Helpers;
using NotasApi.Repositories;

namespace NotasApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CarpetasController : ControllerBase
{
    private readonly ICarpetaRepository _carpetaRepository;

    public CarpetasController(ICarpetaRepository carpetaRepository)
    {
        _carpetaRepository = carpetaRepository;
    }

    [HttpGet("arbol")]
    public async Task<ActionResult> ObtenerArbol()
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            var arbol = await _carpetaRepository.ObtenerArbolAsync(usuarioId);
            return Ok(arbol);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el árbol de carpetas", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult> CrearCarpeta([FromBody] CrearCarpetaRequest request)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            Guid carpetaId;

            if (request.CarpetaPadreId.HasValue)
            {
                carpetaId = await _carpetaRepository.CrearSubcarpetaAsync(
                    usuarioId, 
                    request.CarpetaPadreId.Value, 
                    request.Nombre, 
                    request.Icono, 
                    request.ColorHex
                );
            }
            else
            {
                carpetaId = await _carpetaRepository.CrearRaizAsync(
                    usuarioId, 
                    request.Nombre, 
                    request.Icono, 
                    request.ColorHex
                );
            }

            return CreatedAtAction(nameof(ObtenerArbol), new { id = carpetaId }, new { id = carpetaId });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear la carpeta", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> EliminarCarpeta(Guid id)
    {
        try
        {
            var usuarioId = User.GetUsuarioId();
            await _carpetaRepository.EliminarRamaAsync(usuarioId, id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar la carpeta", error = ex.Message });
        }
    }
}
