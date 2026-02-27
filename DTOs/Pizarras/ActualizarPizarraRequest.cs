namespace NotasApi.DTOs.Pizarras;

public class ActualizarPizarraRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string SceneJson { get; set; } = "{}";
    public Guid? NotaId { get; set; }
    public bool? EsArchivada { get; set; }
}

