namespace OrderFlow.Orders.Api.Models;

public class Producto
{
    public string Sku { get; set; } = default!;
    public string Nombre { get; set; } = default!;
    public int Disponible { get; set; }
}