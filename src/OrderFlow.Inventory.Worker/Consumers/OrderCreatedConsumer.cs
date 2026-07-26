using MassTransit;
using OrderFlow.Contracts;
using OrderFlow.Inventory.Worker.Services;

namespace OrderFlow.Inventory.Worker.Consumers;

public class OrderCreatedConsumer(StockService stockService, ILogger<OrderCreatedConsumer> logger)
    : IConsumer<OrderCreated>
{
    public async Task Consume(ConsumeContext<OrderCreated> context)
    {
        var evt = context.Message;

        logger.LogInformation("Procesando OrderCreated {OrderId} (evento {EventId})", evt.OrderId, evt.EventId);

        var reserva = await stockService.ReservarStockAsync(
            evt.EventId, evt.Sku, evt.Cantidad, context.CancellationToken);

        switch (reserva.Resultado)
        {
            case ResultadoReserva.Reservado:
                await context.Publish(new StockReserved(
                    Guid.NewGuid(), evt.OrderId, evt.Sku, evt.Cantidad, reserva.StockRestante, DateTime.UtcNow));
                logger.LogInformation("Stock reservado para pedido {OrderId}. Restante: {StockRestante}", evt.OrderId, reserva.StockRestante);
                break;

            case ResultadoReserva.Rechazado:
                await context.Publish(new StockRejected(
                    Guid.NewGuid(), evt.OrderId, evt.Sku, evt.Cantidad, "Stock insuficiente", reserva.StockRestante, DateTime.UtcNow));
                logger.LogInformation("Stock rechazado para pedido {OrderId}. Disponible: {StockRestante}", evt.OrderId, reserva.StockRestante);
                break;

            case ResultadoReserva.YaProcesado:
                logger.LogInformation("Evento {EventId} ya procesado previamente, se ignora", evt.EventId);
                break;
        }
    }
}