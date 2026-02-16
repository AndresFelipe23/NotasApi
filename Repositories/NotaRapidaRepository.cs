using Dapper;
using NotasApi.Data;
using NotasApi.Models;

namespace NotasApi.Repositories;

public class NotaRapidaRepository : INotaRapidaRepository
{
    private readonly IDbConnectionFactory _dbFactory;

    public NotaRapidaRepository(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Guid> CrearAsync(Guid usuarioId, string contenido, string? colorHex = null)
    {
        using var connection = _dbFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UsuarioId", usuarioId);
        parameters.Add("@Contenido", contenido);
        parameters.Add("@ColorHex", colorHex);
        parameters.Add("@NuevoId", dbType: System.Data.DbType.Guid, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync("usp_NotasRapidas_Crear", parameters, commandType: System.Data.CommandType.StoredProcedure);
        return parameters.Get<Guid>("@NuevoId");
    }

    public async Task ActualizarAsync(Guid id, Guid usuarioId, string contenido, string? colorHex = null)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_NotasRapidas_Actualizar",
            new { Id = id, UsuarioId = usuarioId, Contenido = contenido, ColorHex = colorHex },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<NotaRapida>> ObtenerTodasAsync(Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryAsync<NotaRapida>(
            "usp_NotasRapidas_ObtenerTodas",
            new { UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task ArchivarAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_NotasRapidas_Archivar",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task EliminarAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_NotasRapidas_Eliminar",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Guid> ConvertirANotaAsync(Guid usuarioId, Guid notaRapidaId)
    {
        using var connection = _dbFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UsuarioId", usuarioId);
        parameters.Add("@NotaRapidaId", notaRapidaId);
        parameters.Add("@NuevaNotaId", dbType: System.Data.DbType.Guid, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync("usp_NotasRapidas_ConvertirANota", parameters, commandType: System.Data.CommandType.StoredProcedure);
        return parameters.Get<Guid>("@NuevaNotaId");
    }
}
