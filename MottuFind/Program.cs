
using System.Text;
using System.Text.Json.Serialization;
using Asp.Versioning;
using HealthChecks.Oracle;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
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
            builder.Services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();

                // 🔐 Adiciona suporte ao botão "Authorize" com JWT
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Insira o token JWT no formato: Bearer {seu token}"
                });

                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
            });


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

            builder.Services.AddScoped<AuthService>();

            // Configuração do JWT
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });

            builder.Services.AddAuthorization();


            builder.Services.AddAutoMapper(typeof(MappingProfile));

            builder.Services.AddControllers()
                .AddJsonOptions(opt => {
                                opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
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
                app.UseSwaggerUI(ui =>
                {
                    ui.SwaggerEndpoint("/swagger/v1/swagger.json", "MottuFind_C_.API v1");
                    ui.SwaggerEndpoint("/swagger/v2/swagger.json", "MottuFind_C_.API v2");
                }
                );
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

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
