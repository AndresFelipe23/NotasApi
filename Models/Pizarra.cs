namespace NotasApi.Models;

public class Pizarra
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid? NotaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string SceneJson { get; set; } = string.Empty;
    public bool EsArchivada { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime FechaActualizacion { get; set; }
}

