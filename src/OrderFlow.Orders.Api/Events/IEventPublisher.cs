using OrderFlow.Contracts;

namespace OrderFlow.Orders.Api.Events;

    public interface IEventPublisher
{
    Task PublishOrderCreatedAsync(OrderCreated evt, CancellationToken ct = default);
}