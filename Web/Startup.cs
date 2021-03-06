﻿using System;
using System.Globalization;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using WebMarkupMin.AspNetCore3;
using WebMarkupMin.AspNet.Common.Compressors;

using DominikStiller.VertretungsplanServer.Models;
using DominikStiller.VertretungsplanServer.Web.Helper;
using DominikStiller.VertretungsplanServer.Helper;
using DominikStiller.VertretungsplanServer.Web.Controllers;

namespace DominikStiller.VertretungsplanServer.Web
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
            CultureInfo.CurrentCulture = new CultureInfo("de");

            services.AddOptions();
            services.Configure<DataLoaderOptions>(Configuration.GetSection("DataLoader"));
            services.Configure<ViewOptions>(Configuration.GetSection("View"));
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                // Requests through Chrome Data Saver cause "Parameter count mismatch between X-Forwarded-For and X-Forwarded-Proto" if true
                options.RequireHeaderSymmetry = false;
            });

            services.AddWebMarkupMin()
                .AddHttpCompression(options =>
                {
                    options.CompressorFactories = new List<ICompressorFactory> { new GZipCompressorFactory() };
                });

            services.AddRazorPages();
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/";
                    options.LogoutPath = "/logout";
                    // Workaround to prevent redirect if access is denied
                    options.Events = new CookieAuthenticationEvents
                    {
                        OnRedirectToAccessDenied = (ctx) =>
                        {
                            ctx.Response.StatusCode = 403;
                            return Task.CompletedTask;
                        }
                    };
                });
            services.AddAuthorization();

            services.AddSingleton<VertretungsplanRepository, VertretungsplanRepository>();
            services.AddSingleton<UserRepository, UserRepository>();
            services.AddSingleton<DataLoader, DataLoader>();
            services.AddSingleton<VertretungsplanHelper, VertretungsplanHelper>();
            services.AddScoped<ResponseCachingHelper, ResponseCachingHelper>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, UserRepository userRepository, DataLoader dataLoader)
        {

            app.UseAuthentication();

            app.UseStatusCodePagesWithReExecute("/error");

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("de"),
                SupportedCultures = new[] { new CultureInfo("de") },
                SupportedUICultures = new[] { new CultureInfo("de") }
            });

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

            app.UseWebMarkupMin();

            app.UseStaticFiles(new StaticFileOptions()
            {
                OnPrepareResponse = context =>
                {
                    var headers = context.Context.Response.GetTypedHeaders();
                    headers.CacheControl = new CacheControlHeaderValue()
                    {
                        Public = true,
                        MaxAge = TimeSpan.FromDays(365)
                    };
                }
            });

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            var userRepositoryConfigSection = Configuration.GetSection("UserRepository");
            userRepository.LoadUsers(userRepositoryConfigSection.GetValue<string>("StudentsAuthDataPath"), UserType.Student);
            userRepository.LoadUsers(userRepositoryConfigSection.GetValue<string>("TeachersAuthDataPath"), UserType.Teacher);

            dataLoader.Start();
        }
    }
}
