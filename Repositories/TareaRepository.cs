using Dapper;
using NotasApi.Data;
using NotasApi.Models;

namespace NotasApi.Repositories;

public class TareaRepository : ITareaRepository
{
    private readonly IDbConnectionFactory _dbFactory;

    public TareaRepository(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Guid> CrearAsync(Guid usuarioId, string descripcion, Guid? notaVinculadaId = null, 
        int prioridad = 2, DateTimeOffset? fechaVencimiento = null)
    {
        using var connection = _dbFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UsuarioId", usuarioId);
        parameters.Add("@Descripcion", descripcion);
        parameters.Add("@NotaVinculadaId", notaVinculadaId);
        parameters.Add("@Prioridad", prioridad);
        parameters.Add("@FechaVencimiento", fechaVencimiento);
        parameters.Add("@NuevoId", dbType: System.Data.DbType.Guid, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync("usp_Tareas_Crear", parameters, commandType: System.Data.CommandType.StoredProcedure);
        return parameters.Get<Guid>("@NuevoId");
    }

    public async Task ActualizarAsync(Guid id, Guid usuarioId, string descripcion, int prioridad, DateTimeOffset? fechaVencimiento = null)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Tareas_Actualizar",
            new
            {
                Id = id,
                UsuarioId = usuarioId,
                Descripcion = descripcion,
                Prioridad = prioridad,
                FechaVencimiento = fechaVencimiento
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task AlternarEstadoAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Tareas_AlternarEstado",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Tarea>> ObtenerPendientesAsync(Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryAsync<Tarea>(
            "usp_Tareas_ObtenerPendientes",
            new { UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<Tarea>> ObtenerCompletadasAsync(Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryAsync<Tarea>(
            "usp_Tareas_ObtenerCompletadas",
            new { UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task EliminarAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Tareas_Eliminar",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
