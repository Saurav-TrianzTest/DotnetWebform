using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Modules;
using log4net;
using log4net.Config;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace eShopLegacyWebForms
{
    public class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure log4net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(builder.Environment.ContentRootPath, "log4Net.xml")));

            // Add configuration
            var configuration = builder.Configuration;

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddSession();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddApplicationInsightsTelemetry();

            // Configure EF Core
            var connectionString = configuration.GetConnectionString("CatalogDBContext");
            builder.Services.AddDbContext<CatalogDBContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(30);
                }));

            // Configure Autofac
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                var mockData = bool.Parse(configuration["UseMockData"] ?? "false");
                containerBuilder.RegisterModule(new ApplicationModule(mockData));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseStatusCodePagesWithReExecute("/Error");
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            // Configure localization
            var supportedCultures = new[] { new CultureInfo("en-US") };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.MapRazorPages();

            // Seed database if not using mock data
            var mockData = bool.Parse(configuration["UseMockData"] ?? "false");
            if (!mockData)
            {
                using (var scope = app.Services.CreateScope())
                {
                    var scopeProvider = scope.ServiceProvider;
                    try
                    {
                        var context = scopeProvider.GetRequiredService<CatalogDBContext>();
                        var initializer = scopeProvider.GetRequiredService<CatalogDBInitializer>();
                        context.Database.Migrate();
                        initializer.Seed(context);
                    }
                    catch (Exception ex)
                    {
                        _log.Error("An error occurred while seeding the database.", ex);
                    }
                }
            }

            _log.Debug("Application starting...");
            app.Run();
        }
    }
}
