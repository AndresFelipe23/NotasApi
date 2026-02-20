using NotasApi.Models;

namespace NotasApi.Repositories;

public interface ITareaRepository
{
    Task<Guid> CrearAsync(Guid usuarioId, string descripcion, Guid? notaVinculadaId = null, 
        int prioridad = 2, DateTimeOffset? fechaVencimiento = null);
    Task ActualizarAsync(Guid id, Guid usuarioId, string descripcion, int prioridad, DateTimeOffset? fechaVencimiento = null);
    Task AlternarEstadoAsync(Guid id, Guid usuarioId);
    Task<IEnumerable<Tarea>> ObtenerPendientesAsync(Guid usuarioId);
    Task<IEnumerable<Tarea>> ObtenerCompletadasAsync(Guid usuarioId);
    Task<IEnumerable<Tarea>> ObtenerPorNotaVinculadaAsync(Guid usuarioId, Guid notaVinculadaId);
    Task EliminarAsync(Guid id, Guid usuarioId);
}
