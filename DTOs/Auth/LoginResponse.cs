namespace NotasApi.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UsuarioInfo Usuario { get; set; } = null!;
}

public class UsuarioInfo
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Apellido { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string? FotoPerfilUrl { get; set; }
}
