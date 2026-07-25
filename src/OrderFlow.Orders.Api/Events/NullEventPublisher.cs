using OrderFlow.Contracts;

namespace OrderFlow.Orders.Api.Events;

public class NullEventPublisher(ILogger<NullEventPublisher> logger) : IEventPublisher
{
    public Task PublishOrderCreatedAsync(OrderCreated evt, CancellationToken ct = default)
    {
        logger.LogInformation("OrderCreated publicado (stub): {@evt}", evt);
        return Task.CompletedTask;
    }
}
