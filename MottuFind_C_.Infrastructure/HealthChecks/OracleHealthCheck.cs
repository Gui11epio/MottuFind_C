using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Sprint1_C_.Infrastructure.Data;

namespace MottuFind_C_.Infrastructure.HealthChecks
{
    public class OracleHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OracleHealthCheck> _logger;

        public OracleHealthCheck(AppDbContext context, ILogger<OracleHealthCheck> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Testa a conexão abrindo uma transação simples
                var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

                if (canConnect)
                {
                    return HealthCheckResult.Healthy("Oracle database connection is healthy.");
                }

                return HealthCheckResult.Unhealthy("Cannot connect to Oracle database.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oracle health check failed.");
                return HealthCheckResult.Unhealthy(
                    "Oracle health check failed",
                    ex,
                    new Dictionary<string, object>
                    {
                        { "error", ex.Message }
                    });
            }
        }
    }
}
