using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;

namespace BankingApp.WebApi
{
    public static class SwaggerConfig
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "BankingApp API",
                    Version = "v1",
                    Description = "API documentation for Banking Application with Hexagonal Architecture"
                });
            });

            return services;
        }
    }

}
