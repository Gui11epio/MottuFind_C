using Microsoft.OpenApi.Models;
using MottuFind_C_.Application.Config;

namespace MottuFind.Extensions
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services, SwaggerSettings settings)
        {
            return services.AddSwaggerGen(swagger =>
            {
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = settings.Title,
                    Version = "v1",
                    Description = settings.Description,
                    Contact = new OpenApiContact
                    {
                        Name = settings.Contact.Name,
                        Email = settings.Contact.Email,
                    }
                });

                swagger.SwaggerDoc("v2", new OpenApiInfo
                {
                    Title = settings.Title,
                    Version = "v2",
                    Description = settings.Description,
                    Contact = new OpenApiContact
                    {
                        Name = settings.Contact.Name,
                        Email = settings.Contact.Email,
                    }
                });

                swagger.EnableAnnotations();
            }
            );
        }
    }
}
