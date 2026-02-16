namespace NotasApi.Models;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Apellido { get; set; } // Nullable porque en SQL es NULL
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FotoPerfilUrl { get; set; }
    public bool EsActivo { get; set; }
    public DateTime? FechaUltimoAcceso { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
