using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Worker.Data;
using OrderFlow.Inventory.Worker.Models;
using OrderFlow.Inventory.Worker.Services;
using Xunit;

namespace OrderFlow.Inventory.Tests;

public class StockServiceTests
{
    private static InventoryDbContext CrearDbEnMemoria()
    {
        // Cada test usa su propia conexión SQLite en memoria, abierta durante
        // toda la vida del test (si se cierra, la base desaparece).
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new InventoryDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task ReservarStock_ConStockSuficiente_DescuentaYDevuelveReservado()
    {
        using var db = CrearDbEnMemoria();
        db.Stock.Add(new Stock { Sku = "ABC-01", Nombre = "Teclado", Disponible = 10 });
        await db.SaveChangesAsync();

        var service = new StockService(db);
        var resultado = await service.ReservarStockAsync(Guid.NewGuid(), "ABC-01", 3);

        Assert.Equal(ResultadoReserva.Reservado, resultado);
        Assert.Equal(7, db.Stock.First(s => s.Sku == "ABC-01").Disponible);
    }

    [Fact]
    public async Task ReservarStock_SinStockSuficiente_Rechaza()
    {
        using var db = CrearDbEnMemoria();
        db.Stock.Add(new Stock { Sku = "ABC-01", Nombre = "Teclado", Disponible = 2 });
        await db.SaveChangesAsync();

        var service = new StockService(db);
        var resultado = await service.ReservarStockAsync(Guid.NewGuid(), "ABC-01", 5);

        Assert.Equal(ResultadoReserva.Rechazado, resultado);
        Assert.Equal(2, db.Stock.First(s => s.Sku == "ABC-01").Disponible); // no se tocó
    }

    [Fact]
    public async Task ReservarStock_MismoEventIdDosVeces_SoloDescuentaUnaVez()
    {
        using var db = CrearDbEnMemoria();
        db.Stock.Add(new Stock { Sku = "ABC-01", Nombre = "Teclado", Disponible = 10 });
        await db.SaveChangesAsync();

        var service = new StockService(db);
        var eventId = Guid.NewGuid();

        var primerIntento = await service.ReservarStockAsync(eventId, "ABC-01", 3);
        var segundoIntento = await service.ReservarStockAsync(eventId, "ABC-01", 3); // mismo eventId

        Assert.Equal(ResultadoReserva.Reservado, primerIntento);
        Assert.Equal(ResultadoReserva.YaProcesado, segundoIntento);
        Assert.Equal(7, db.Stock.First(s => s.Sku == "ABC-01").Disponible); // no 4
    }
}