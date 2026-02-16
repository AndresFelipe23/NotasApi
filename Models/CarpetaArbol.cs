namespace NotasApi.Models;

public class CarpetaArbol
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public string? ColorHex { get; set; }
    public short Nivel { get; set; } 
    public string RutaString { get; set; } = string.Empty; // El HIERARCHYID convertido a texto!
    public int Orden { get; set; }
}
