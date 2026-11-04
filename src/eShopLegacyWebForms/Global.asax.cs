using Autofac;
using eShopLegacyWebForms.Models;
using eShopLegacyWebForms.Models.Infrastructure;
using eShopLegacyWebForms.Modules;
using log4net;
using System;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;

namespace eShopLegacyWebForms
{
    public class Global : HttpApplication
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static IContainerProvider _containerProvider;
        IContainer container;

        public IContainerProvider ContainerProvider
        {
            get { return _containerProvider; }
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            // In cloud environments with multiple instances, Application_Start fires
            // on each instance. Configuration is externalized to environment variables
            // and AWS Systems Manager Parameter Store for consistent initialization.
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            ConfigureContainer();
            ConfigDataBase();
        }

        /// <summary>
        /// Track the machine name and the start time for the session inside the current session
        /// </summary>
        protected void Session_Start(Object sender, EventArgs e)
        {
            var instanceId = GetCloudInstanceId();
            HttpContext.Current.Session["MachineName"] = instanceId;
            HttpContext.Current.Session["SessionStartTime"] = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// http://docs.autofac.org/en/latest/integration/webforms.html
        /// </summary>
        private void ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            var mockData = bool.Parse(ConfigurationManager.AppSettings["UseMockData"] ?? Environment.GetEnvironmentVariable("UseMockData") ?? "false");
            builder.RegisterModule(new ApplicationModule(mockData));
            container = builder.Build();
            _containerProvider = new ContainerProvider(container);
        }

        private void ConfigDataBase()
        {
            var mockData = bool.Parse(ConfigurationManager.AppSettings["UseMockData"] ?? Environment.GetEnvironmentVariable("UseMockData") ?? "false");

            if (!mockData)
            {
                Database.SetInitializer<CatalogDBContext>(container.Resolve<CatalogDBInitializer>());
            }
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            //set the property to our new object
            LogicalThreadContext.Properties["activityid"] = new ActivityIdHelper();

            LogicalThreadContext.Properties["requestinfo"] = new WebRequestInfo();

            _log.Debug("Application_BeginRequest");
        }

        private static string GetCloudInstanceId()
        {
            // In cloud environments, machine names are ephemeral and randomly generated.
            // Use ECS Task metadata endpoint when available, fall back to machine name.
            var ecsMetadataUri = Environment.GetEnvironmentVariable("ECS_CONTAINER_METADATA_URI_V4");
            if (!string.IsNullOrEmpty(ecsMetadataUri))
            {
                try
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        client.Timeout = TimeSpan.FromSeconds(2);
                        var taskJson = client.GetStringAsync(ecsMetadataUri + "/task").Result;
                        var doc = System.Text.Json.JsonDocument.Parse(taskJson);
                        var taskArn = doc.RootElement.GetProperty("TaskARN").GetString() ?? "";
                        var parts = taskArn.Split('/');
                        return parts.Length > 0 ? parts[parts.Length - 1] : Environment.MachineName;
                    }
                }
                catch
                {
                    // Fall back to machine name if metadata endpoint is unavailable
                }
            }
            return Environment.MachineName;
        }
    }

    public class ActivityIdHelper
    {
        public override string ToString()
        {
            if (Trace.CorrelationManager.ActivityId == Guid.Empty)
            {
                Trace.CorrelationManager.ActivityId = Guid.NewGuid();
            }

            return Trace.CorrelationManager.ActivityId.ToString();
        }

    }
    public class WebRequestInfo
    {
        private readonly string _rawUrl;
        private readonly string _userAgent;

        public WebRequestInfo()
        {
            var context = HttpContext.Current;
            _rawUrl = context?.Request?.RawUrl ?? string.Empty;
            _userAgent = context?.Request?.UserAgent ?? string.Empty;
        }

        public override string ToString()
        {
            return _rawUrl + ", " + _userAgent;
        }
    }
}