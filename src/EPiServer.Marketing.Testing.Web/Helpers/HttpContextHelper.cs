using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    /// <summary>
    /// interacts with the httpcontext for reading and manipulating the objects therein
    /// </summary>
    [ServiceConfiguration(ServiceType = typeof(IHttpContextHelper), Lifecycle = ServiceInstanceScope.Singleton)]

    [ExcludeFromCodeCoverage]
    internal class HttpContextHelper : IHttpContextHelper
    {
        public bool HasItem(string itemId)
        {
            return HttpContext.Current.Items.Contains(itemId);
        }

        public string GetRequestParam(string itemId)
        {
            return HttpContext.Current.Request.Params[itemId];
        }

        public void SetItemValue(string itemId, object value)
        {
            HttpContext.Current.Items[itemId] = value;
        }

        public void RemoveItem(string itemId)
        {
            HttpContext.Current.Items.Remove(itemId);
        }

        public bool HasCookie(string cookieKey)
        {
            return HttpContext.Current.Response.Cookies.AllKeys.Contains(cookieKey);
        }

        public string GetCookieValue(string cookieKey)
        {
            return HttpContext.Current.Response.Cookies[cookieKey].Value;
        }

        public HttpCookie GetResponseCookie(string cookieKey)
        {
            return HttpContext.Current.Response.Cookies.Get(cookieKey);
        }

        public HttpCookie GetRequestCookie(string cookieKey)
        {
            return HttpContext.Current.Request.Cookies.Get(cookieKey);
        }

        public string[] GetResponseCookieKeys()
        {
            return HttpContext.Current.Response.Cookies.AllKeys;
        }

        public string[] GetRequestCookieKeys()
        {
            return HttpContext.Current.Request.Cookies.AllKeys;
        }

        public void RemoveCookie(string cookieKey)
        {
            HttpContext.Current.Response.Cookies.Remove(cookieKey);
            HttpContext.Current.Request.Cookies.Remove(cookieKey);
        }

        public void AddCookie(HttpCookie cookie)
        {
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public bool CanWriteToResponse()
        {
            return HttpContext.Current.Response.Filter.CanWrite;
        }

        public Stream GetResponseFilter()
        {
            return HttpContext.Current.Response.Filter;
        }

        public void SetResponseFilter(Stream stream)
        {
            HttpContext.Current.Response.Filter = stream;
        }

        public bool HasCurrentContext()
        {
            return HttpContext.Current != null;
        }

        public bool HasUserAgent()
        {
            return HttpContext.Current.Request.UserAgent != null;
        }

        public string RequestedUrl()
        {
            return HttpContext.Current.Request.RawUrl;
        }

        public HttpContext GetCurrentContext()
        {
            return HttpContext.Current;
        }
    }
}
