using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Worker.Data;
using OrderFlow.Inventory.Worker.Models;

namespace OrderFlow.Inventory.Worker.Services;

public enum ResultadoReserva { Reservado, Rechazado, YaProcesado }

public record ReservaResult(ResultadoReserva Resultado, int StockRestante);

public class StockService(InventoryDbContext db)
{
    public async Task<ReservaResult> ReservarStockAsync(
        Guid eventId, string sku, int cantidad, CancellationToken ct = default)
    {
        var yaProcesado = await db.EventosProcesados
            .AnyAsync(e => e.EventId == eventId, ct);

        if (yaProcesado)
        {
            // El stock actual, para informar aunque no se vuelva a tocar.
            var stockActual = await db.Stock.FirstOrDefaultAsync(s => s.Sku == sku, ct);
            return new ReservaResult(ResultadoReserva.YaProcesado, stockActual?.Disponible ?? 0);
        }

        var stock = await db.Stock.FirstOrDefaultAsync(s => s.Sku == sku, ct);

        ResultadoReserva resultado;
        int stockRestante;

        if (stock is null || stock.Disponible < cantidad)
        {
            resultado = ResultadoReserva.Rechazado;
            stockRestante = stock?.Disponible ?? 0;
        }
        else
        {
            stock.Disponible -= cantidad;
            resultado = ResultadoReserva.Reservado;
            stockRestante = stock.Disponible;
        }

        db.EventosProcesados.Add(new EventoProcesado { EventId = eventId });
        await db.SaveChangesAsync(ct);

        return new ReservaResult(resultado, stockRestante);
    }
}