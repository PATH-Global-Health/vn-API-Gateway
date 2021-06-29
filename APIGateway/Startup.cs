using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using APIGateway.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

namespace APIGateway
{
    public class Startup
    {

        public Startup(IHostEnvironment env)
        {
            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
            builder.SetBasePath(env.ContentRootPath)
                   .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
                   .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => {
                return true;
            };
            services.AddControllers();
            services.AddCors(options =>
            {
                var origins = new string[]
                {
                    "https://cds.hcdc.vn",
                    "http://localhost:3000",
                    "http://test.hcdc.vn",
                    "http://abcde.hcdc.vn"
                };
                options.AddDefaultPolicy(
                                  builder =>
                                  {
                                      builder.WithOrigins(origins)
                                             .AllowAnyHeader()
                                             .AllowAnyMethod()
                                             .AllowCredentials();
                                  });
            });
            services.AddOcelot(Configuration).AddConsul();
            // Register the Swagger services
            services.AddSwaggerDocument();
            Console.WriteLine("v1.0.1");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            app.UseOcelot().Wait();
        }
    }
}
