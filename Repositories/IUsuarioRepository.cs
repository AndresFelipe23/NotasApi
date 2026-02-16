using NotasApi.Models;

namespace NotasApi.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(Guid id);
    Task<Usuario?> ObtenerPorCorreoAsync(string correo);
    Task<Guid> CrearAsync(Usuario usuario);
    Task ActualizarAsync(Usuario usuario);
    Task ActualizarUltimoAccesoAsync(Guid id);
}
