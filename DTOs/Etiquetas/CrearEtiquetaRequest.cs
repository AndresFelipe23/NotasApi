namespace NotasApi.DTOs.Etiquetas;

public class CrearEtiquetaRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
}
