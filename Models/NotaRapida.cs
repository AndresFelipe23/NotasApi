namespace NotasApi.Models;

public class NotaRapida
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
    public bool EsArchivada { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
