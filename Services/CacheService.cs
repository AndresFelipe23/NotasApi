using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace NotasApi.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(15);

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var cachedValue = await _cache.GetStringAsync(key);
            if (string.IsNullOrEmpty(cachedValue))
                return null;

            return JsonSerializer.Deserialize<T>(cachedValue);
        }
        catch (Exception ex)
        {
            // Si Redis no está disponible, simplemente retornar null (sin caché)
            _logger?.LogWarning(ex, "Error al obtener del caché para la clave {Key}. Continuando sin caché.", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };

            var serializedValue = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, serializedValue, options);
        }
        catch (Exception ex)
        {
            // Si Redis no está disponible, simplemente ignorar (sin caché)
            _logger?.LogWarning(ex, "Error al guardar en caché para la clave {Key}. Continuando sin caché.", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _cache.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            // Si Redis no está disponible, simplemente ignorar
            _logger?.LogWarning(ex, "Error al eliminar del caché para la clave {Key}. Continuando sin caché.", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        // Nota: Redis no soporta directamente búsqueda por patrón en DistributedCache
        // Para producción, considera usar StackExchange.Redis directamente para SCAN
        // Por ahora, invalidamos manualmente las claves conocidas
        // Esto se puede mejorar con un servicio de Redis más avanzado si es necesario
        await Task.CompletedTask;
    }
}
