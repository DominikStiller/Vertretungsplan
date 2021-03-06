﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, DataLoader dataLoader)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else if (env.IsProduction())
            {
                loggerFactory.AddAWSProvider(Configuration.GetAWSLoggingConfigSection());
                app.UseErrorLogging();

                var rewriteOptions = new RewriteOptions()
                    .AddRedirectToHttpsPermanent();
                app.UseRewriter(rewriteOptions);
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            _ = dataLoader.LoadDataFromS3();
        }
    }
}
