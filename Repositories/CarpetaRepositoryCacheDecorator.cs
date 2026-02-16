using NotasApi.Models;
using NotasApi.Services;

namespace NotasApi.Repositories;

public class CarpetaRepositoryCacheDecorator : ICarpetaRepository
{
    private readonly ICarpetaRepository _repository;
    private readonly ICacheService _cache;

    public CarpetaRepositoryCacheDecorator(ICarpetaRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Guid> CrearRaizAsync(Guid usuarioId, string nombre, string? icono = null, string? colorHex = null)
    {
        var result = await _repository.CrearRaizAsync(usuarioId, nombre, icono, colorHex);
        // Invalidar caché del árbol de carpetas
        await _cache.RemoveAsync(CacheKeys.CarpetaArbol(usuarioId));
        return result;
    }

    public async Task<Guid> CrearSubcarpetaAsync(Guid usuarioId, Guid carpetaPadreId, string nombre, string? icono = null, string? colorHex = null)
    {
        var result = await _repository.CrearSubcarpetaAsync(usuarioId, carpetaPadreId, nombre, icono, colorHex);
        // Invalidar caché del árbol de carpetas
        await _cache.RemoveAsync(CacheKeys.CarpetaArbol(usuarioId));
        return result;
    }

    public async Task<IEnumerable<CarpetaArbol>> ObtenerArbolAsync(Guid usuarioId)
    {
        var cacheKey = CacheKeys.CarpetaArbol(usuarioId);
        
        // Intentar obtener del caché
        var cached = await _cache.GetAsync<List<CarpetaArbol>>(cacheKey);
        if (cached != null)
            return cached;

        // Si no está en caché, obtener de la base de datos
        var result = await _repository.ObtenerArbolAsync(usuarioId);
        var resultList = result.ToList();

        // Guardar en caché (15 minutos - el árbol de carpetas no cambia frecuentemente)
        await _cache.SetAsync(cacheKey, resultList, TimeSpan.FromMinutes(15));

        return resultList;
    }

    public async Task EliminarRamaAsync(Guid usuarioId, Guid carpetaId)
    {
        await _repository.EliminarRamaAsync(usuarioId, carpetaId);
        // Invalidar caché del árbol de carpetas
        await _cache.RemoveAsync(CacheKeys.CarpetaArbol(usuarioId));
    }
}
