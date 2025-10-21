
using System.Text.Json.Serialization;
using HealthChecks.Oracle;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MottuFind.Extensions;
using MottuFind_C_.Application.Services;
using MottuFind_C_.Domain.Repositories;
using MottuFind_C_.Infrastructure.HealthChecks;
using MottuFind_C_.Infrastructure.Repositories;
using Sprint1_C_.Application.Services;
using Sprint1_C_.Infrastructure.Data;
using Sprint1_C_.Mappings;
using OracleHealthCheck = MottuFind_C_.Infrastructure.HealthChecks.OracleHealthCheck;

namespace Sprint1_C_
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(o =>
                o.EnableAnnotations()
            );

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = Environment.GetEnvironmentVariable("DEFAULT_CONNECTION");
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new Exception("A vari�vel de ambiente DEFAULT_CONNECTION n�o est� definida.");

                options.UseOracle(connectionString);
            });

            builder.Services.AddScoped<IMotoRepository, MotoRepository>();
            builder.Services.AddScoped<MotoService>();

            builder.Services.AddScoped<IFilialRepository, FilialRepository>();
            builder.Services.AddScoped<FilialService>();

            builder.Services.AddScoped<IPatioRepository, PatioRepository>();
            builder.Services.AddScoped<PatioService>();

            builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            builder.Services.AddScoped<UsuarioService>();

            builder.Services.AddScoped<ILeitorRfidRepository, LeitorRfidRepository>();
            builder.Services.AddScoped<LeitorRfidService>();

            builder.Services.AddScoped<ILeituraRfidRepository, LeituraRfidRepository>();
            builder.Services.AddScoped<LeituraRfidService>();


            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.AddControllers()
                .AddJsonOptions(opt => {
                                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // HEALTH CHECKS SIMPLIFICADO
            builder.Services.AddHealthChecks()
                .AddCheck<ApplicationHealthCheck>(
                    "Application",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "application", "internal" }
                )
                .AddCheck<OracleHealthCheck>(
                    "Oracle Database",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "database" }
                );

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseRouting();

            // ENDPOINTS DE HEALTH CHECK
            app.MapHealthChecks("/health", new HealthCheckOptions()
            {
                ResponseWriter = HealthCheckExtensions.WriteResponse,
                Predicate = check => check.Tags.Contains("application") ||
                                   check.Tags.Contains("database") ||
                                   check.Tags.Contains("external")
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions()
            {
                ResponseWriter = HealthCheckExtensions.WriteResponse,
                Predicate = check => check.Tags.Contains("database") ||
                                   check.Tags.Contains("external")
            });

            app.MapHealthChecks("/health/live", new HealthCheckOptions()
            {
                ResponseWriter = HealthCheckExtensions.WriteResponse,
                Predicate = check => check.Tags.Contains("application")
            });

            app.Run();
        }
    }
}
