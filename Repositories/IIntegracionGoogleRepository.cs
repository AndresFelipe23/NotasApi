using NotasApi.Models;

namespace NotasApi.Repositories;

public interface IIntegracionGoogleRepository
{
    Task UpsertAsync(IntegracionGoogle integracion);
    Task<IntegracionGoogle?> ObtenerPorUsuarioAsync(Guid usuarioId);
    Task ActualizarTokensAsync(Guid usuarioId, string accessToken, DateTimeOffset tokenExpiresAt);
    Task DesconectarAsync(Guid usuarioId);
}
