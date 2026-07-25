using MassTransit;
using MassTransit.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Contracts;
using OrderFlow.Orders.Api.Consumers;
using OrderFlow.Orders.Api.Data;
using OrderFlow.Orders.Api.Models;
using Xunit;

namespace OrderFlow.Orders.Tests;

public class StockConsumersTests
{
    private static (ServiceProvider provider, SqliteConnection connection) CrearProveedor()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddDbContext<OrdersDbContext>(opt => opt.UseSqlite(connection));

        services.AddMassTransitTestHarness(x =>
        {
            x.AddConsumer<StockReservedConsumer>();
            x.AddConsumer<StockRejectedConsumer>();
        });

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<OrdersDbContext>().Database.EnsureCreated();

        return (provider, connection);
    }

    [Fact]
    public async Task StockReserved_ConfirmaPedidoPendiente()
    {
        var (provider, connection) = CrearProveedor();
        using var _ = connection;

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var pedidoId = Guid.NewGuid();
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
            db.Pedidos.Add(new Pedido
            {
                Id = pedidoId,
                ClienteNombre = "Test",
                Sku = "ABC-01",
                Cantidad = 1,
                Estado = EstadoPedido.Pending
            });
            await db.SaveChangesAsync();
        }

        await harness.Bus.Publish(new StockReserved(Guid.NewGuid(), pedidoId, "ABC-01", 1, DateTime.UtcNow));

        Assert.True(await harness.Consumed.Any<StockReserved>());

        using var scopeFinal = provider.CreateScope();
        var pedido = await scopeFinal.ServiceProvider.GetRequiredService<OrdersDbContext>().Pedidos.FindAsync(pedidoId);

        Assert.Equal(EstadoPedido.Confirmed, pedido!.Estado);

        await harness.Stop();
    }

    [Fact]
    public async Task StockRejected_NoSobreescribeUnPedidoQueYaNoEstaPendiente()
    {
        var (provider, connection) = CrearProveedor();
        using var _ = connection;

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var pedidoId = Guid.NewGuid();
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
            db.Pedidos.Add(new Pedido
            {
                Id = pedidoId,
                ClienteNombre = "Test",
                Sku = "ABC-01",
                Cantidad = 1,
                Estado = EstadoPedido.Confirmed // ya no está Pending
            });
            await db.SaveChangesAsync();
        }

        await harness.Bus.Publish(new StockRejected(Guid.NewGuid(), pedidoId, "ABC-01", 1, "Stock insuficiente", DateTime.UtcNow));

        Assert.True(await harness.Consumed.Any<StockRejected>());

        using var scopeFinal = provider.CreateScope();
        var pedido = await scopeFinal.ServiceProvider.GetRequiredService<OrdersDbContext>().Pedidos.FindAsync(pedidoId);

        // La guarda WHERE Estado == Pending debe impedir que un StockRejected tardío
        // sobreescriba un pedido que ya fue confirmado por otro camino.
        Assert.Equal(EstadoPedido.Confirmed, pedido!.Estado);

        await harness.Stop();
    }
}