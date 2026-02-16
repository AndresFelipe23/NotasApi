namespace NotasApi.DTOs.Tareas;

public class CrearTareaRequest
{
    public string Descripcion { get; set; } = string.Empty;
    public Guid? NotaVinculadaId { get; set; }
    public int Prioridad { get; set; } = 2; // 1=Alta, 2=Media, 3=Baja
    public DateTimeOffset? FechaVencimiento { get; set; }
}
