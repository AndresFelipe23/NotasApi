namespace NotasApi.Models;

public class Tarea
{
    public Guid Id { get; set; }
    public Guid UsuarioId { get; set; }
    public Guid? NotaVinculadaId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool EstaCompletada { get; set; }
    public int Prioridad { get; set; }
    public int Orden { get; set; }
    
    // Mapea perfectamente con el DATETIMEOFFSET de SQL Server
    public DateTimeOffset? FechaVencimiento { get; set; } 
    
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaCompletada { get; set; }
}
