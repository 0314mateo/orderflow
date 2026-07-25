using OrderFlow.Orders.Api.Models;

namespace OrderFlow.Orders.Api.Data;
public static class SeedData
{
    public static void Seed(OrdersDbContext db)
    {
        if (db.Catalogo.Any()) return;

        db.Catalogo.AddRange(
            new Producto { Sku = "ABC-01", Nombre = "Teclado mecánico", Disponible = 50 },
            new Producto { Sku = "ABC-02", Nombre = "Mouse inalámbrico", Disponible = 100 },
            new Producto { Sku = "ABC-03", Nombre = "Monitor 24\"", Disponible = 20 }
        );
        db.SaveChanges();
    }
}
