using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Contracts;
using OrderFlow.Orders.Api.Data;
using OrderFlow.Orders.Api.Models;

namespace OrderFlow.Orders.Api.Consumers;

public class StockRejectedConsumer(OrdersDbContext db, ILogger<StockRejectedConsumer> logger)
    : IConsumer<StockRejected>
{
    public async Task Consume(ConsumeContext<StockRejected> context)
    {
        var evt = context.Message;

        var filasAfectadas = await db.Pedidos
            .Where(p => p.Id == evt.OrderId && p.Estado == EstadoPedido.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(p => p.Estado, EstadoPedido.Rejected)
                .SetProperty(p => p.Detalle, evt.Motivo)
                .SetProperty(p => p.StockRestante, evt.StockRestante));

        if (filasAfectadas > 0)
            logger.LogInformation("Pedido {OrderId} rechazado: {Motivo}", evt.OrderId, evt.Motivo);
        else
            logger.LogInformation("Pedido {OrderId} no estaba en Pending, se ignora StockRejected", evt.OrderId);
    }
}