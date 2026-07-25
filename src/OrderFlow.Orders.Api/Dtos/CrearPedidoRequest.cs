namespace OrderFlow.Orders.Api.Dtos;

public record CrearPedidoRequest(string ClienteNombre, string Sku, int Cantidad);
