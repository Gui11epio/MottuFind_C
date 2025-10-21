using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace MottuFind_C_.Infrastructure.HealthChecks;

public class ApplicationHealthCheck : IHealthCheck
{
    private readonly ILogger<ApplicationHealthCheck> _logger;

    public ApplicationHealthCheck(ILogger<ApplicationHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verifique aqui a saúde da sua aplicação
            // Exemplo: verificar memória, conexões, etc.

            var memoryInfo = GC.GetGCMemoryInfo();
            var totalMemory = GC.GetTotalMemory(false) / 1024 / 1024; // MB

            var data = new Dictionary<string, object>
            {
                { "memory_used_mb", totalMemory },
                { "gc_heap_size_mb", memoryInfo.HeapSizeBytes / 1024 / 1024 },
                { "timestamp", DateTime.UtcNow }
            };

            if (totalMemory > 500) // Exemplo: alerta se usar mais de 500MB
            {
                return Task.FromResult(
                    HealthCheckResult.Degraded(
                        "High memory usage",
                        data: data
                    ));
            }

            return Task.FromResult(
                HealthCheckResult.Healthy(
                    "Application is healthy",
                    data
                ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return Task.FromResult(
                HealthCheckResult.Unhealthy(
                    "Application health check failed",
                    ex,
                    new Dictionary<string, object> { { "error", ex.Message } }
                ));
        }
    }
}