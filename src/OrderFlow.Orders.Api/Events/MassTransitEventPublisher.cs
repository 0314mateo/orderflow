using MassTransit;
using OrderFlow.Contracts;

namespace OrderFlow.Orders.Api.Events;

public class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public async Task PublishOrderCreatedAsync(OrderCreated evt, CancellationToken ct = default)
    {
        await publishEndpoint.Publish(evt, ct);
    }
}