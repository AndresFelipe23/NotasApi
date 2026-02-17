using Dapper;
using NotasApi.Data;
using NotasApi.Models;

namespace NotasApi.Repositories;

public class NotaRepository : INotaRepository
{
    private readonly IDbConnectionFactory _dbFactory;

    public NotaRepository(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Guid> CrearAsync(Guid usuarioId, Guid? carpetaId, string titulo, string? resumen = null, 
        string? icono = null, string? contenidoBloques = null)
    {
        using var connection = _dbFactory.CreateConnection();
        var parameters = new DynamicParameters();
        parameters.Add("@UsuarioId", usuarioId);
        parameters.Add("@CarpetaId", carpetaId);
        parameters.Add("@Titulo", titulo);
        parameters.Add("@Resumen", resumen);
        parameters.Add("@Icono", icono);
        parameters.Add("@ContenidoBloques", contenidoBloques);
        parameters.Add("@NuevoId", dbType: System.Data.DbType.Guid, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync("usp_Notas_Crear", parameters, commandType: System.Data.CommandType.StoredProcedure);
        return parameters.Get<Guid>("@NuevoId");
    }

    public async Task ActualizarAsync(Guid id, Guid usuarioId, string titulo, string? resumen = null, 
        string? icono = null, string? imagenPortadaUrl = null, string? contenidoBloques = null)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Notas_Actualizar",
            new
            {
                Id = id,
                UsuarioId = usuarioId,
                Titulo = titulo,
                Resumen = resumen,
                Icono = icono,
                ImagenPortadaUrl = imagenPortadaUrl,
                ContenidoBloques = contenidoBloques
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Nota?> ObtenerPorIdAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Nota>(
            "usp_Notas_ObtenerPorId",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<NotaResumen>> ObtenerResumenPorCarpetaAsync(Guid usuarioId, Guid? carpetaId = null)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryAsync<NotaResumen>(
            "usp_Notas_ObtenerResumenPorCarpeta",
            new { UsuarioId = usuarioId, CarpetaId = carpetaId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<NotaResumen>> ObtenerResumenTodasAsync(Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryAsync<NotaResumen>(
            "usp_Notas_ObtenerResumenTodas",
            new { UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task MoverACarpetaAsync(Guid id, Guid usuarioId, Guid? nuevaCarpetaId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Notas_MoverACarpeta",
            new { Id = id, UsuarioId = usuarioId, NuevaCarpetaId = nuevaCarpetaId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task AlternarFavoritoAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Notas_AlternarFavorito",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task ArchivarAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Notas_Archivar",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<IEnumerable<NotaResumen>> ObtenerArchivadasAsync(Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryAsync<NotaResumen>(
            "usp_Notas_ObtenerArchivadas",
            new { UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task RecuperarAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Notas_Recuperar",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task EliminarAsync(Guid id, Guid usuarioId)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Notas_Eliminar",
            new { Id = id, UsuarioId = usuarioId },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
