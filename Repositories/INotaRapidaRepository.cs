using NotasApi.Models;

namespace NotasApi.Repositories;

public interface INotaRapidaRepository
{
    Task<Guid> CrearAsync(Guid usuarioId, string contenido, string? colorHex = null);
    Task ActualizarAsync(Guid id, Guid usuarioId, string contenido, string? colorHex = null);
    Task<IEnumerable<NotaRapida>> ObtenerTodasAsync(Guid usuarioId);
    Task ArchivarAsync(Guid id, Guid usuarioId);
    Task EliminarAsync(Guid id, Guid usuarioId);
    Task<Guid> ConvertirANotaAsync(Guid usuarioId, Guid notaRapidaId);
}
