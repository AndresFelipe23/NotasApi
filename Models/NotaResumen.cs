namespace NotasApi.Models;

public class NotaResumen
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Resumen { get; set; }
    public string? Icono { get; set; }
    public string? ImagenPortadaUrl { get; set; }
    public bool EsFavorita { get; set; }
    public bool EsPublica { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
