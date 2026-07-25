namespace OrderFlow.Contracts;

public record OrderCreated(Guid EventId, Guid OrderId, string Sku, int Cantidad, DateTime OcurridoEn);