namespace NotasApi.DTOs.Notas;

public class CrearNotaRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string? Resumen { get; set; }
    public string? Icono { get; set; }
    public string? ContenidoBloques { get; set; }
    public Guid? CarpetaId { get; set; }
}
