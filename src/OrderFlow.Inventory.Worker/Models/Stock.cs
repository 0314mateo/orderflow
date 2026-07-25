namespace OrderFlow.Inventory.Worker.Models;

public class Stock
{
    public string Sku { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public int Disponible { get; set; }
}