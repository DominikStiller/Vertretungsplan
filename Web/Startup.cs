using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;

using DominikStiller.VertretungsplanServer.Models;
using DominikStiller.VertretungsplanServer.Web.Helper;
using DominikStiller.VertretungsplanServer.Helper;

namespace DominikStiller.VertretungsplanServer.Web
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            CultureInfo.CurrentCulture = new CultureInfo("de");

            services.AddOptions();
            services.Configure<DataLoaderOptions>(Configuration.GetSection("DataLoader"));

            services.AddMvc();

            services.AddSingleton<VertretungsplanRepository, VertretungsplanRepository>();
            services.AddSingleton<DataLoader, DataLoader>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, DataLoader dataLoader)
        {
            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug();
                app.UseDeveloperExceptionPage();
            }
            else if (env.IsProduction())
            {
                loggerFactory.AddAWSProvider(Configuration.GetAWSLoggingConfigSection());
                app.UseMiddleware<ErrorLoggingMiddleware>();
            }

            app.UseStaticFiles();

            app.UseMvc();

            dataLoader.Start();
        }
    }
}
