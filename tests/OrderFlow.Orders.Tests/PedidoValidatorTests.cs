using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Api.Data;
using OrderFlow.Orders.Api.Dtos;
using OrderFlow.Orders.Api.Models;
using OrderFlow.Orders.Api.Validation;
using Xunit;

namespace OrderFlow.Orders.Tests;

public class PedidoValidatorTests
{
    private static OrdersDbContext CrearDbConCatalogo()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new OrdersDbContext(options);
        db.Database.EnsureCreated();
        db.Catalogo.Add(new Producto { Sku = "ABC-01", Nombre = "Teclado", Disponible = 50 });
        db.SaveChanges();

        return db;
    }

    [Fact]
    public void Validar_PedidoValido_NoDevuelveErrores()
    {
        using var db = CrearDbConCatalogo();
        var request = new CrearPedidoRequest("Juan Pérez", "ABC-01", 2);

        var errores = PedidoValidator.Validar(request, db);

        Assert.Empty(errores);
    }

    [Fact]
    public void Validar_ClienteNombreVacio_DevuelveError()
    {
        using var db = CrearDbConCatalogo();
        var request = new CrearPedidoRequest("", "ABC-01", 2);

        var errores = PedidoValidator.Validar(request, db);

        Assert.Contains(errores, e => e.Contains("clienteNombre"));
    }

    [Fact]
    public void Validar_SkuInexistente_DevuelveError()
    {
        using var db = CrearDbConCatalogo();
        var request = new CrearPedidoRequest("Juan Pérez", "ZZZ-99", 2);

        var errores = PedidoValidator.Validar(request, db);

        Assert.Contains(errores, e => e.Contains("ZZZ-99"));
    }

    [Fact]
    public void Validar_CantidadFueraDeRango_DevuelveError()
    {
        using var db = CrearDbConCatalogo();
        var request = new CrearPedidoRequest("Juan Pérez", "ABC-01", 500);

        var errores = PedidoValidator.Validar(request, db);

        Assert.Contains(errores, e => e.Contains("cantidad"));
    }
}