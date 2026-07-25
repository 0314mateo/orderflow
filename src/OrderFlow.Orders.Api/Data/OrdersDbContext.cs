using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Api.Models;

namespace OrderFlow.Orders.Api.Data;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<Producto> Catalogo => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Producto>().HasKey(p => p.Sku);
    }
}
