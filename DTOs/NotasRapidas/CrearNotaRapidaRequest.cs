namespace NotasApi.DTOs.NotasRapidas;

public class CrearNotaRapidaRequest
{
    public string Contenido { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
}
