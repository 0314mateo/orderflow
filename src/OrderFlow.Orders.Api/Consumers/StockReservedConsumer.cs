using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts;
using OrderFlow.Orders.Api.Data;
using OrderFlow.Orders.Api.Models;

namespace OrderFlow.Orders.Api.Consumers;

public class StockReservedConsumer(OrdersDbContext db, ILogger<StockReservedConsumer> logger)
    : IConsumer<StockReserved>
{
    public async Task Consume(ConsumeContext<StockReserved> context)
    {
        var evt = context.Message;

        var filasAfectadas = await db.Pedidos
            .Where(p => p.Id == evt.OrderId && p.Estado == EstadoPedido.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Estado, EstadoPedido.Confirmed));

        if (filasAfectadas > 0)
            logger.LogInformation("Pedido {OrderId} confirmado", evt.OrderId);
        else
            logger.LogInformation("Pedido {OrderId} no estaba en Pending, se ignora StockReserved", evt.OrderId);
    }
}