namespace NotasApi.DTOs.Etiquetas;

public class ActualizarEtiquetaRequest
{
    public string Nombre { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
}
