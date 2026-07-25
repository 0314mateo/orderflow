using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Orders.Api.Data;
using OrderFlow.Orders.Api.Dtos;
using OrderFlow.Orders.Api.Events;
using OrderFlow.Orders.Api.Models;
using OrderFlow.Orders.Api.Validation;
using OrderFlow.Contracts;
using OrderFlow.Orders.Api.Consumers;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<OrdersDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("OrdersDb")));
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<StockReservedConsumer>();
    x.AddConsumer<StockRejectedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

var app = builder.Build();
app.UseCors("Frontend");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    db.Database.EnsureCreated();
    SeedData.Seed(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "OrderFlow Orders API");
    });
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapPost("/orders", async (CrearPedidoRequest req, OrdersDbContext db, IEventPublisher publisher, ILogger<Program> logger) =>
{
    var errores = PedidoValidator.Validar(req, db);
    if (errores.Count > 0)
        return Results.BadRequest(new { errores });

    var pedido = new Pedido
    {
        ClienteNombre = req.ClienteNombre,
        Sku = req.Sku,
        Cantidad = req.Cantidad
    };

    db.Pedidos.Add(pedido);
    await db.SaveChangesAsync();

    try
    {
        await publisher.PublishOrderCreatedAsync(new OrderCreated(
            EventId: Guid.NewGuid(),
            OrderId: pedido.Id,
            Sku: pedido.Sku,
            Cantidad: pedido.Cantidad,
            OcurridoEn: DateTime.UtcNow));
    }
    catch (Exception ex)
    {
        logger.LogError(ex,
            "No se pudo publicar OrderCreated para el pedido {OrderId}. El pedido quedó creado en Pending, " +
            "pero no será procesado por Inventory hasta que se reconcilie manualmente.",
            pedido.Id);
    }

    return Results.Created($"/orders/{pedido.Id}", pedido);
});

app.MapGet("/orders", async (OrdersDbContext db) =>
    await db.Pedidos.OrderByDescending(p => p.CreadoEn).ToListAsync());

app.MapGet("/orders/{id:guid}", async (Guid id, OrdersDbContext db) =>
    await db.Pedidos.FindAsync(id) is { } pedido
        ? Results.Ok(pedido)
        : Results.NotFound());

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
