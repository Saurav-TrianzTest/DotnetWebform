using System;
using System.Collections.Generic;
using System.Web;

namespace eShopLegacyWebForms.Modules
{
    public class CspHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PostReleaseRequestState += OnPostReleaseRequestState;
        }

        private void OnPostReleaseRequestState(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var response = app.Context.Response;

            // Content Security Policy headers to prevent XSS and data injection attacks
            var cspDirectives = new List<string>
            {
                "default-src 'self'",
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' ajax.googleapis.com ajax.aspnetcdn.com",
                "style-src 'self' 'unsafe-inline' fonts.googleapis.com",
                "img-src 'self' data: https:",
                "font-src 'self' fonts.gstatic.com",
                "connect-src 'self'",
                "object-src 'none'",
                "base-uri 'self'",
                "frame-ancestors 'none'"
            };

            response.Headers["Content-Security-Policy"] = string.Join("; ", cspDirectives);
            response.Headers["X-Content-Type-Options"] = "nosniff";
            response.Headers["X-Frame-Options"] = "DENY";
            response.Headers["X-XSS-Protection"] = "1; mode=block";
        }

        public void Dispose()
        {
        }
    }
}
