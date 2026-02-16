using Dapper;
using NotasApi.Data;
using NotasApi.Models;

namespace NotasApi.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _dbFactory;

    public UsuarioRepository(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<Usuario?> ObtenerPorIdAsync(Guid id)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Usuario>(
            "usp_Usuarios_ObtenerPorId",
            new { Id = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Usuario?> ObtenerPorCorreoAsync(string correo)
    {
        using var connection = _dbFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Usuario>(
            "usp_Usuarios_ObtenerPorCorreo",
            new { Correo = correo },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<Guid> CrearAsync(Usuario usuario)
    {
        using var connection = _dbFactory.CreateConnection();
        usuario.Id = Guid.NewGuid();

        await connection.ExecuteAsync(
            "usp_Usuarios_Crear",
            new
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                PasswordHash = usuario.PasswordHash,
                FotoPerfilUrl = usuario.FotoPerfilUrl,
                EsActivo = usuario.EsActivo
            },
            commandType: System.Data.CommandType.StoredProcedure);

        return usuario.Id;
    }

    public async Task ActualizarAsync(Usuario usuario)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Usuarios_Actualizar",
            new
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                PasswordHash = usuario.PasswordHash,
                FotoPerfilUrl = usuario.FotoPerfilUrl,
                EsActivo = usuario.EsActivo
            },
            commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task ActualizarUltimoAccesoAsync(Guid id)
    {
        using var connection = _dbFactory.CreateConnection();
        await connection.ExecuteAsync(
            "usp_Usuarios_ActualizarUltimoAcceso",
            new { Id = id },
            commandType: System.Data.CommandType.StoredProcedure);
    }
}
