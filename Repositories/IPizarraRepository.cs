using NotasApi.Models;

namespace NotasApi.Repositories;

public interface IPizarraRepository
{
    Task<Guid> CrearAsync(Guid usuarioId, string titulo, string? descripcion, string sceneJson, Guid? notaId);
    Task ActualizarAsync(Guid id, Guid usuarioId, string titulo, string? descripcion, string sceneJson, Guid? notaId, bool? esArchivada);
    Task<Pizarra?> ObtenerPorIdAsync(Guid id, Guid usuarioId);
    Task<IEnumerable<Pizarra>> ObtenerTodasAsync(Guid usuarioId, bool incluirArchivadas);
    Task<IEnumerable<Pizarra>> ObtenerPorNotaAsync(Guid usuarioId, Guid notaId);
    Task ArchivarAsync(Guid id, Guid usuarioId);
    Task RecuperarAsync(Guid id, Guid usuarioId);
    Task EliminarAsync(Guid id, Guid usuarioId);
}

