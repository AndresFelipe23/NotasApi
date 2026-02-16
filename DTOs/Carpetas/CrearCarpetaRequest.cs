namespace NotasApi.DTOs.Carpetas;

public class CrearCarpetaRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public string? ColorHex { get; set; }
    public Guid? CarpetaPadreId { get; set; } // Si es null, crea una carpeta raíz
}
