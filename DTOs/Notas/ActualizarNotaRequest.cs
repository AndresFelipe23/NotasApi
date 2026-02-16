namespace NotasApi.DTOs.Notas;

public class ActualizarNotaRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string? Resumen { get; set; }
    public string? Icono { get; set; }
    public string? ImagenPortadaUrl { get; set; }
    public string? ContenidoBloques { get; set; }
}
