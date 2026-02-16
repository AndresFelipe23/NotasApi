namespace NotasApi.Models;

public class Nota
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid? CarpetaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Resumen { get; set; }
    public string? Icono { get; set; }
    public string? ImagenPortadaUrl { get; set; }
    public string? ContenidoBloques { get; set; } // JSON del editor
    public bool EsFavorita { get; set; }
    public bool EsArchivada { get; set; }
    public bool EsPublica { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
