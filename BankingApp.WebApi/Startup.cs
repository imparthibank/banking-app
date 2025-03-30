using BankingApp.Application.Ports.Input;
using BankingApp.Application.UseCases;
using BankingApp.Application.Validation;
using BankingApp.Core.Ports.Output;
using BankingApp.Infrastructure.Adapters.Output.Repositories;
using BankingApp.Infrastructure.Config;
using BankingApp.WebApi.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BankingApp.WebApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen();

            services.AddSingleton<ConnectionFactory>();
            services.AddScoped<IBankAccountRepository, BankAccountRepository>();
            services.AddScoped<IBankAccountService, BankAccountService>();
            services.AddScoped<IBankAccountValidator, BankAccountValidator>();
            services.AddSwaggerDocumentation();

            services.AddAutoMapper(typeof(Startup).Assembly);
            services.AddLogging(); // 👈 Adds logging support


        }


        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BankingApp API v1");
                    c.RoutePrefix = "swagger"; // URL will be /swagger
                });
            }

            app.UseRouting();
            app.UseMiddleware<ExceptionMiddleware>(); // <-- Add this before UseRouting
            app.UseAuthorization();
            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
