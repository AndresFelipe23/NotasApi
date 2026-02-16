using NotasApi.Models;

namespace NotasApi.Repositories;

public interface ICarpetaRepository
{
    Task<Guid> CrearRaizAsync(Guid usuarioId, string nombre, string? icono = null, string? colorHex = null);
    Task<Guid> CrearSubcarpetaAsync(Guid usuarioId, Guid carpetaPadreId, string nombre, string? icono = null, string? colorHex = null);
    Task<IEnumerable<CarpetaArbol>> ObtenerArbolAsync(Guid usuarioId);
    Task EliminarRamaAsync(Guid usuarioId, Guid carpetaId);
}
