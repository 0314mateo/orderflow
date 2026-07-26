namespace OrderFlow.Orders.Api.Models;

public enum EstadoPedido { Pending, Confirmed, Rejected }

public class Pedido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ClienteNombre { get; set; } = default!;
    public string Sku { get; set; } = default!;
    public int Cantidad { get; set; }
    public EstadoPedido Estado { get; set; } = EstadoPedido.Pending;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public string? Detalle { get; set; }
    public int? StockRestante { get; set; }
}