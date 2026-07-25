namespace OrderFlow.Inventory.Worker;

public class Worker(ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Inventory Worker iniciado.");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Cancelación esperada al detener el servicio (Ctrl+C, docker stop, etc.)
            logger.LogInformation("Inventory Worker deteniéndose de forma controlada.");
        }
    }
}