namespace NotasApi.Models;

public class Etiqueta
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public DateTime FechaCreacion { get; set; }
}
