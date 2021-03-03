using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.Elasticsearch;
using Serilog.Events;


namespace APIGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseSerilog((ctx, config) =>
                    {
                        config
                            .MinimumLevel.Verbose()
                            .Enrich.FromLogContext()
                            .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("https://elasticsearch-es-http:9200"))
                            {
                                AutoRegisterTemplate = true,
                                ModifyConnectionSettings = x => x.ServerCertificateValidationCallback((o, certificate, arg3, arg4) => { return true; })
                                                                 .BasicAuthentication("elastic", "9UM40HQ6hy540aup1xZb0q6r"),
                            });

                    });
                });
    }
}
