using Dapper;
using NotasApi.Data;
using NotasApi.Models;

namespace NotasApi.Repositories;

public class PizarraRepository : IPizarraRepository
{
    private readonly IDbConnectionFactory _dbFactory;

    public PizarraRepository(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Guid> CrearAsync(Guid usuarioId, string titulo, string? descripcion, string sceneJson, Guid? notaId)
    {
        using var connection = _dbFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UsuarioId", usuarioId);
        parameters.Add("@Titulo", titulo);
        parameters.Add("@Descripcion", descripcion);
        parameters.Add("@SceneJson", sceneJson);
        parameters.Add("@NotaId", notaId);
        parameters.Add("@NuevoId", dbType: System.Data.DbType.Guid, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync("usp_Pizarras_Crear", parameters, commandType: System.Data.CommandType.StoredProcedure);
        return parameters.Get<Guid>("@NuevoId");
    }

    public async Task ActualizarAsync(Guid id, Guid usuarioId, string titulo, string? descripcion, string sceneJson, Guid? notaId, bool? esArchivada)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Pizarras_Actualizar",
            new
            {
                Id = id,
                UsuarioId = usuarioId,
                Titulo = titulo,
                Descripcion = descripcion,
                SceneJson = sceneJson,
                NotaId = notaId,
                EsArchivada = esArchivada
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Pizarra?> ObtenerPorIdAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Pizarra>(
            "usp_Pizarras_ObtenerPorId",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Pizarra>> ObtenerTodasAsync(Guid usuarioId, bool incluirArchivadas)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryAsync<Pizarra>(
            "usp_Pizarras_ObtenerTodas",
            new { UsuarioId = usuarioId, IncluirArchivadas = incluirArchivadas },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Pizarra>> ObtenerPorNotaAsync(Guid usuarioId, Guid notaId)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryAsync<Pizarra>(
            "usp_Pizarras_ObtenerPorNota",
            new { UsuarioId = usuarioId, NotaId = notaId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task ArchivarAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Pizarras_Archivar",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task RecuperarAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Pizarras_Recuperar",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task EliminarAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Pizarras_Eliminar",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}

