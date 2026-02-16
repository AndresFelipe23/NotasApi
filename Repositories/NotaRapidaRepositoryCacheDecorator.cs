using NotasApi.Models;
using NotasApi.Services;

namespace NotasApi.Repositories;

public class NotaRapidaRepositoryCacheDecorator : INotaRapidaRepository
{
    private readonly INotaRapidaRepository _repository;
    private readonly ICacheService _cache;

    public NotaRapidaRepositoryCacheDecorator(INotaRapidaRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Guid> CrearAsync(Guid usuarioId, string contenido, string? colorHex = null)
    {
        var result = await _repository.CrearAsync(usuarioId, contenido, colorHex);
        // Invalidar caché de notas rápidas
        await _cache.RemoveAsync(CacheKeys.NotasRapidas(usuarioId));
        return result;
    }

    public async Task ActualizarAsync(Guid id, Guid usuarioId, string contenido, string? colorHex = null)
    {
        await _repository.ActualizarAsync(id, usuarioId, contenido, colorHex);
        // Invalidar caché de notas rápidas
        await _cache.RemoveAsync(CacheKeys.NotasRapidas(usuarioId));
    }

    public async Task<IEnumerable<NotaRapida>> ObtenerTodasAsync(Guid usuarioId)
    {
        var cacheKey = CacheKeys.NotasRapidas(usuarioId);
        
        // Intentar obtener del caché
        var cached = await _cache.GetAsync<List<NotaRapida>>(cacheKey);
        if (cached != null)
            return cached;

        // Si no está en caché, obtener de la base de datos
        var result = await _repository.ObtenerTodasAsync(usuarioId);
        var resultList = result.ToList();

        // Guardar en caché (5 minutos - las notas rápidas cambian más frecuentemente)
        await _cache.SetAsync(cacheKey, resultList, TimeSpan.FromMinutes(5));

        return resultList;
    }

    public async Task ArchivarAsync(Guid id, Guid usuarioId)
    {
        await _repository.ArchivarAsync(id, usuarioId);
        // Invalidar caché de notas rápidas
        await _cache.RemoveAsync(CacheKeys.NotasRapidas(usuarioId));
    }

    public async Task EliminarAsync(Guid id, Guid usuarioId)
    {
        await _repository.EliminarAsync(id, usuarioId);
        // Invalidar caché de notas rápidas
        await _cache.RemoveAsync(CacheKeys.NotasRapidas(usuarioId));
    }

    public async Task<Guid> ConvertirANotaAsync(Guid usuarioId, Guid notaRapidaId)
    {
        var result = await _repository.ConvertirANotaAsync(usuarioId, notaRapidaId);
        // Invalidar caché de notas rápidas
        await _cache.RemoveAsync(CacheKeys.NotasRapidas(usuarioId));
        return result;
    }
}
