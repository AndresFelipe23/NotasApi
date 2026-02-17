namespace NotasApi.DTOs.Etiquetas;

public class AsignarEtiquetasRequest
{
    public List<Guid> EtiquetaIds { get; set; } = new();
}
