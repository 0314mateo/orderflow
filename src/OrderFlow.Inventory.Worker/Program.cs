using Microsoft.EntityFrameworkCore;
using OrderFlow.Inventory.Worker;
using OrderFlow.Inventory.Worker.Data;
using OrderFlow.Inventory.Worker.Services;
using MassTransit;
using OrderFlow.Inventory.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<InventoryDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("InventoryDb")));
builder.Services.AddScoped<StockService>();
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderCreatedConsumer>();

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

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    db.Database.EnsureCreated();
    SeedData.Seed(db);
}

host.Run();