using OrderFlow.Inventory.Worker.Models;

namespace OrderFlow.Inventory.Worker.Data;

public static class SeedData
{
    public static void Seed(InventoryDbContext db)
    {
        if (db.Stock.Any()) return;

        db.Stock.AddRange(
            new Stock { Sku = "ABC-01", Nombre = "Teclado mecánico", Disponible = 50 },
            new Stock { Sku = "ABC-02", Nombre = "Mouse inalámbrico", Disponible = 100 },
            new Stock { Sku = "ABC-03", Nombre = "Monitor 24\"", Disponible = 20 }
        );
        db.SaveChanges();
    }
}