using NotasApi.Models;

namespace NotasApi.Repositories;

public interface INotaRepository
{
    Task<Guid> CrearAsync(Guid usuarioId, Guid? carpetaId, string titulo, string? resumen = null, 
        string? icono = null, string? contenidoBloques = null);
    Task ActualizarAsync(Guid id, Guid usuarioId, string titulo, string? resumen = null, 
        string? icono = null, string? imagenPortadaUrl = null, string? contenidoBloques = null);
    Task<Nota?> ObtenerPorIdAsync(Guid id, Guid usuarioId);
    Task<IEnumerable<NotaResumen>> ObtenerResumenPorCarpetaAsync(Guid usuarioId, Guid? carpetaId = null);
    Task MoverACarpetaAsync(Guid id, Guid usuarioId, Guid? nuevaCarpetaId);
    Task AlternarFavoritoAsync(Guid id, Guid usuarioId);
    Task ArchivarAsync(Guid id, Guid usuarioId);
}
