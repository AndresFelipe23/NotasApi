namespace NotasApi.Models;

public class IntegracionGoogle
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset TokenExpiresAt { get; set; }
    public string? GoogleUserId { get; set; }
    public string? GoogleEmail { get; set; }
    public string? GoogleName { get; set; }
    public bool EstaActiva { get; set; }
    public DateTimeOffset FechaConectada { get; set; }
    public DateTimeOffset FechaActualizacion { get; set; }
}
