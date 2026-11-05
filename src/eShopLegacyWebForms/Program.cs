using Autofac;
using Autofac.Extensions.DependencyInjection;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Modules;
using eShopLegacyWebForms.Services;
using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]


namespace eShopLegacyWebForms
{
    public class Program
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Program));

        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            // Configure log4net
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            var mockData = bool.Parse(configuration["AppSettings:UseMockData"] ?? "false");

            var host = Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseConfiguration(configuration);
                })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddConfiguration(configuration);
                })
                .Build();

            // Run database migration and seeding if not using mock data
            if (!mockData)
            {
                using (var scope = host.Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<CatalogDBContext>();
                    dbContext.Database.Migrate();

                    var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
                    initializer.Seed(dbContext);
                }
            }

            host.Run();
        }
    }

    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var mockData = bool.Parse(_configuration["AppSettings:UseMockData"] ?? "false");

            services.AddControllersWithViews()
                .AddNewtonsoftJson();

            services.AddRazorPages();

            services.AddSession();

            services.AddApplicationInsightsTelemetry();

            // Register CatalogDBContext
            services.AddDbContext<CatalogDBContext>(options =>
                options.UseSqlServer(_configuration.GetConnectionString("CatalogDBContext")));

            // Register services
            if (mockData)
            {
                services.AddSingleton<ICatalogService, CatalogServiceMock>();
            }
            else
            {
                services.AddScoped<ICatalogService, CatalogService>();
            }

            services.AddSingleton<CatalogItemHiLoGenerator>();
            services.AddScoped<CatalogDBInitializer>();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            var mockData = bool.Parse(_configuration["AppSettings:UseMockData"] ?? "false");
            builder.RegisterModule(new ApplicationModule(mockData));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            // Configure routes equivalent to RouteConfig
            // Routes are now handled by ASP.NET Core routing above
        }
    }
}
