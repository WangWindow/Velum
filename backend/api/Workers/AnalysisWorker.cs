using Velum.Core.Interfaces;
using Velum.Base.Data;

namespace Velum.Api.Workers;

public class AnalysisWorker(ILogger<AnalysisWorker> logger, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var openAIService = scope.ServiceProvider.GetRequiredService<IOpenAIService>();

                // TODO: Implement analysis logic here
                // Example: Fetch pending tasks, analyze using OpenAI, save results

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Analysis worker running at: {time}", DateTimeOffset.Now);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in analysis worker");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
