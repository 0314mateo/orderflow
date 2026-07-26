namespace OrderFlow.Contracts;

public record StockRejected(Guid EventId, Guid OrderId, string Sku, int Cantidad, string Motivo, int StockRestante, DateTime OcurridoEn);