using Dapper;
using NotasApi.Data;
using NotasApi.Models;

namespace NotasApi.Repositories;

public class CarpetaRepository : ICarpetaRepository
{
    private readonly IDbConnectionFactory _dbFactory;

    public CarpetaRepository(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Guid> CrearRaizAsync(Guid usuarioId, string nombre, string? icono = null, string? colorHex = null)
    {
        using var connection = _dbFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UsuarioId", usuarioId);
        parameters.Add("@Nombre", nombre);
        parameters.Add("@Icono", icono);
        parameters.Add("@ColorHex", colorHex);
        parameters.Add("@NuevoId", dbType: System.Data.DbType.Guid, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync("usp_Carpetas_CrearRaiz", parameters, commandType: System.Data.CommandType.StoredProcedure);
        return parameters.Get<Guid>("@NuevoId");
    }

    public async Task<Guid> CrearSubcarpetaAsync(Guid usuarioId, Guid carpetaPadreId, string nombre, string? icono = null, string? colorHex = null)
    {
        using var connection = _dbFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UsuarioId", usuarioId);
        parameters.Add("@CarpetaPadreId", carpetaPadreId);
        parameters.Add("@Nombre", nombre);
        parameters.Add("@Icono", icono);
        parameters.Add("@ColorHex", colorHex);
        parameters.Add("@NuevoId", dbType: System.Data.DbType.Guid, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync("usp_Carpetas_CrearSubcarpeta", parameters, commandType: System.Data.CommandType.StoredProcedure);
        return parameters.Get<Guid>("@NuevoId");
    }

    public async Task<IEnumerable<CarpetaArbol>> ObtenerArbolAsync(Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryAsync<CarpetaArbol>(
            "usp_Carpetas_ObtenerArbol",
            new { UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task EliminarRamaAsync(Guid usuarioId, Guid carpetaId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Carpetas_EliminarRama",
            new { UsuarioId = usuarioId, CarpetaId = carpetaId },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
