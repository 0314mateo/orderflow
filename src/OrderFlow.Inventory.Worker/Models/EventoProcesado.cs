namespace OrderFlow.Inventory.Worker.Models;

public class EventoProcesado
{
    public Guid EventId { get; set; }
    public DateTime ProcesadoEn { get; set; } = DateTime.UtcNow;
}