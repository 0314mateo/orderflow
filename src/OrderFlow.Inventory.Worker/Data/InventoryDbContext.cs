using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Worker.Models;

namespace OrderFlow.Inventory.Worker.Data;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Stock> Stock => Set<Stock>();
    public DbSet<EventoProcesado> EventosProcesados => Set<EventoProcesado>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Stock>().HasKey(s => s.Sku);
        builder.Entity<EventoProcesado>().HasKey(e => e.EventId);
    }
}