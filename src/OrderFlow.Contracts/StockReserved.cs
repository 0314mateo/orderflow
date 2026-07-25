namespace OrderFlow.Contracts;

public record StockReserved(Guid EventId, Guid OrderId, string Sku, int Cantidad, DateTime OcurridoEn);