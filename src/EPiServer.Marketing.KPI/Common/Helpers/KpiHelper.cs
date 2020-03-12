﻿using System;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace EPiServer.Marketing.KPI.Common.Helpers
{
    /// <summary>
    /// This exists to allow us to mock the request for unit testing purposes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [ServiceConfiguration(ServiceType = typeof(IKpiHelper), Lifecycle = ServiceInstanceScope.Singleton)]
    internal class KpiHelper : IKpiHelper
    {
        /// <summary>
        /// Evaluates current URL to determine if page is in a system folder context (e.g Edit, or Preview)
        /// </summary>
        /// <returns></returns>
        public virtual bool IsInSystemFolder()
        {
            var inSystemFolder = true;

            if (HttpContext.Current != null)
            {
                inSystemFolder = HttpContext.Current.Request.RawUrl
                    .IndexOf(Shell.Paths.ProtectedRootPath, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            return inSystemFolder;
        }

        public string GetUrl(ContentReference contentReference)
        {
            return UrlResolver.Current.GetUrl(contentReference);
        }

        public string GetRequestPath()
        {
            return HttpContext.Current!=null ? HttpContext.Current.Request.Path : string.Empty;
        }
    }
}