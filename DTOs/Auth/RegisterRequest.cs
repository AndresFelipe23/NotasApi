namespace NotasApi.DTOs.Auth;

public class RegisterRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Apellido { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
