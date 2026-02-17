using NotasApi.Models;

namespace NotasApi.Repositories;

public interface IEtiquetaRepository
{
    Task<IEnumerable<Etiqueta>> ObtenerPorUsuarioAsync(Guid usuarioId);
    Task<Guid> CrearAsync(Guid usuarioId, string nombre, string? colorHex = null);
    Task ActualizarAsync(Guid id, Guid usuarioId, string nombre, string? colorHex = null);
    Task EliminarAsync(Guid id, Guid usuarioId);
    Task<IEnumerable<Etiqueta>> ObtenerPorNotaAsync(Guid notaId);
    Task AsignarANotaAsync(Guid notaId, IEnumerable<Guid> etiquetaIds);
}
