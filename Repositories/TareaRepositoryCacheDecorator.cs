using NotasApi.Models;
using NotasApi.Services;

namespace NotasApi.Repositories;

public class TareaRepositoryCacheDecorator : ITareaRepository
{
    private readonly ITareaRepository _repository;
    private readonly ICacheService _cache;

    public TareaRepositoryCacheDecorator(ITareaRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Guid> CrearAsync(Guid usuarioId, string descripcion, Guid? notaVinculadaId = null, 
        int prioridad = 2, DateTimeOffset? fechaVencimiento = null)
    {
        var result = await _repository.CrearAsync(usuarioId, descripcion, notaVinculadaId, prioridad, fechaVencimiento);
        // Invalidar caché de tareas pendientes
        await _cache.RemoveAsync(CacheKeys.TareasPendientes(usuarioId));
        return result;
    }

    public async Task ActualizarAsync(Guid id, Guid usuarioId, string descripcion, int prioridad, DateTimeOffset? fechaVencimiento = null)
    {
        await _repository.ActualizarAsync(id, usuarioId, descripcion, prioridad, fechaVencimiento);
        // Invalidar caché de tareas pendientes
        await _cache.RemoveAsync(CacheKeys.TareasPendientes(usuarioId));
    }

    public async Task AlternarEstadoAsync(Guid id, Guid usuarioId)
    {
        await _repository.AlternarEstadoAsync(id, usuarioId);
        await _cache.RemoveAsync(CacheKeys.TareasPendientes(usuarioId));
        await _cache.RemoveAsync(CacheKeys.TareasCompletadas(usuarioId));
    }

    public async Task<IEnumerable<Tarea>> ObtenerPendientesAsync(Guid usuarioId)
    {
        var cacheKey = CacheKeys.TareasPendientes(usuarioId);
        
        // Intentar obtener del caché
        var cached = await _cache.GetAsync<List<Tarea>>(cacheKey);
        if (cached != null)
            return cached;

        // Si no está en caché, obtener de la base de datos
        var result = await _repository.ObtenerPendientesAsync(usuarioId);
        var resultList = result.ToList();

        // Guardar en caché (5 minutos - las tareas cambian frecuentemente)
        await _cache.SetAsync(cacheKey, resultList, TimeSpan.FromMinutes(5));

        return resultList;
    }

    public async Task<IEnumerable<Tarea>> ObtenerCompletadasAsync(Guid usuarioId)
    {
        var cacheKey = CacheKeys.TareasCompletadas(usuarioId);
        var cached = await _cache.GetAsync<List<Tarea>>(cacheKey);
        if (cached != null)
            return cached;

        var result = await _repository.ObtenerCompletadasAsync(usuarioId);
        var resultList = result.ToList();
        await _cache.SetAsync(cacheKey, resultList, TimeSpan.FromMinutes(5));
        return resultList;
    }

    public async Task EliminarAsync(Guid id, Guid usuarioId)
    {
        await _repository.EliminarAsync(id, usuarioId);
        await _cache.RemoveAsync(CacheKeys.TareasPendientes(usuarioId));
        await _cache.RemoveAsync(CacheKeys.TareasCompletadas(usuarioId));
    }
}
