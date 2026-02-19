using Dapper;
using NotasApi.Data;
using NotasApi.Models;

namespace NotasApi.Repositories;

public class IntegracionGoogleRepository : IIntegracionGoogleRepository
{
    private readonly IDbConnectionFactory _dbFactory;

    public IntegracionGoogleRepository(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task UpsertAsync(IntegracionGoogle integracion)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_IntegracionesGoogle_Upsert",
            new
            {
                integracion.UsuarioId,
                integracion.AccessToken,
                integracion.RefreshToken,
                integracion.TokenExpiresAt,
                integracion.GoogleUserId,
                integracion.GoogleEmail,
                integracion.GoogleName
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IntegracionGoogle?> ObtenerPorUsuarioAsync(Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<IntegracionGoogle>(
            "usp_IntegracionesGoogle_ObtenerPorUsuario",
            new { UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
        return result;
    }

    public async Task ActualizarTokensAsync(Guid usuarioId, string accessToken, DateTimeOffset tokenExpiresAt)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_IntegracionesGoogle_ActualizarTokens",
            new { UsuarioId = usuarioId, AccessToken = accessToken, TokenExpiresAt = tokenExpiresAt },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task DesconectarAsync(Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_IntegracionesGoogle_Desconectar",
            new { UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
