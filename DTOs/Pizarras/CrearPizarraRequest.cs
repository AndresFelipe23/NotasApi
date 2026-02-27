namespace NotasApi.DTOs.Pizarras;

public class CrearPizarraRequest
{
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string SceneJson { get; set; } = "{}";
    public Guid? NotaId { get; set; }
}

