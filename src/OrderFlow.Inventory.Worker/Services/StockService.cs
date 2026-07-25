using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Worker.Data;
using OrderFlow.Inventory.Worker.Models;

namespace OrderFlow.Inventory.Worker.Services;

public enum ResultadoReserva { Reservado, Rechazado, YaProcesado }

public class StockService(InventoryDbContext db)
{
    public async Task<ResultadoReserva> ReservarStockAsync(
        Guid eventId, string sku, int cantidad, CancellationToken ct = default)
    {
        // 1. Idempotencia: si este evento ya se procesó, no volver a descontar.
        var yaProcesado = await db.EventosProcesados
            .AnyAsync(e => e.EventId == eventId, ct);

        if (yaProcesado)
            return ResultadoReserva.YaProcesado;

        // 2. Buscar el producto en el inventario real.
        var stock = await db.Stock.FirstOrDefaultAsync(s => s.Sku == sku, ct);

        ResultadoReserva resultado;

        if (stock is null || stock.Disponible < cantidad)
        {
            resultado = ResultadoReserva.Rechazado;
        }
        else
        {
            stock.Disponible -= cantidad;
            resultado = ResultadoReserva.Reservado;
        }

        // 3. Registrar el evento como procesado, pase lo que pase (reservado o rechazado).
        db.EventosProcesados.Add(new EventoProcesado { EventId = eventId });

        await db.SaveChangesAsync(ct);

        return resultado;
    }
}