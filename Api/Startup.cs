using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using DominikStiller.VertretungsplanServer.Api.Helper;
using DominikStiller.VertretungsplanServer.Helper;
using DominikStiller.VertretungsplanServer.Api.Controllers;
using DominikStiller.VertretungsplanServer.Models;

namespace DominikStiller.VertretungsplanServer.Api
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<DataLoaderOptions>(Configuration.GetSection("DataLoader"));
            services.Configure<VertretungsplanControllerOptions>(Configuration.GetSection("VertretungsplanController"));
            services.Configure<NotifierOptions>(Configuration.GetSection("Notifier"));

            services.AddMvc();

            services.AddSingleton<VertretungsplanRepository, VertretungsplanRepository>();
            services.AddSingleton<DataLoader, DataLoader>();
            services.AddSingleton<Notifier, Notifier>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, DataLoader dataLoader)
        {
            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug();
            }
            else if (env.IsProduction())
            {
                loggerFactory.AddAWSProvider(Configuration.GetAWSLoggingConfigSection());
                app.UseMiddleware<ErrorLoggingMiddleware>();
            }

            app.UseMvc();

            dataLoader.LoadDataFromS3();
        }
    }
}
