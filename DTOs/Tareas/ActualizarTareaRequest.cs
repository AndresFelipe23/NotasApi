namespace NotasApi.DTOs.Tareas;

public class ActualizarTareaRequest
{
    public string Descripcion { get; set; } = string.Empty;
    public int Prioridad { get; set; } = 2;
    public DateTimeOffset? FechaVencimiento { get; set; }
}
