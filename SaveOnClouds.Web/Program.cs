using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SaveOnClouds.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json", false).Build();

            var esHost = config["Logging:ElasticSearchLogging:Host"];
            var filePath = config["Logging:FileLogging:Path"];
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); }).UseSerilog(
                    (context, config) =>
                    {
                        if (context.HostingEnvironment.IsDevelopment())
                        {
                            
                            config.WriteTo.File(filePath);
                        }

                        config.WriteTo.Elasticsearch(esHost);
                    });
        }
    }
}