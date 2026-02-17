using System.Text.Json;
using Dapper;
using NotasApi.Data;
using NotasApi.Models;

namespace NotasApi.Repositories;

public class EtiquetaRepository : IEtiquetaRepository
{
    private readonly IDbConnectionFactory _dbFactory;

    public EtiquetaRepository(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<Etiqueta>> ObtenerPorUsuarioAsync(Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryAsync<Etiqueta>(
            "usp_Etiquetas_ObtenerPorUsuario",
            new { UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Guid> CrearAsync(Guid usuarioId, string nombre, string? colorHex = null)
    {
        using var connection = _dbFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UsuarioId", usuarioId);
        parameters.Add("@Nombre", nombre);
        parameters.Add("@ColorHex", colorHex);
        parameters.Add("@NuevoId", dbType: System.Data.DbType.Guid, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync("usp_Etiquetas_Crear", parameters, commandType: System.Data.CommandType.StoredProcedure);
        return parameters.Get<Guid>("@NuevoId");
    }

    public async Task ActualizarAsync(Guid id, Guid usuarioId, string nombre, string? colorHex = null)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Etiquetas_Actualizar",
            new { Id = id, UsuarioId = usuarioId, Nombre = nombre, ColorHex = colorHex },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task EliminarAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Etiquetas_Eliminar",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Etiqueta>> ObtenerPorNotaAsync(Guid notaId)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryAsync<Etiqueta>(
            "usp_NotasEtiquetas_ObtenerPorNota",
            new { NotaId = notaId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task AsignarANotaAsync(Guid notaId, IEnumerable<Guid> etiquetaIds)
    {
        var json = JsonSerializer.Serialize(etiquetaIds.Select(g => g.ToString()).ToArray());
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_NotasEtiquetas_Asignar",
            new { NotaId = notaId, EtiquetaIds = json },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
