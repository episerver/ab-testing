using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    /// <summary>
    /// interacts with the httpcontext for reading and manipulating the objects therein
    /// </summary>
    public class HttpContextHelper : IHttpContextHelper
    {
        public bool HasItem(string itemId)
        {
            return HttpContext.Current.Items.Contains(itemId);
        }

        public void SetItemValue(string itemId, object value)
        {
            HttpContext.Current.Items[itemId] = value;
        }

        public bool HasCookie(string cookieKey)
        {
            return HttpContext.Current.Response.Cookies.AllKeys.Contains(cookieKey);
        }

        public string GetCookieValue(string cookieKey)
        {
            return HttpContext.Current.Response.Cookies[cookieKey].Value;
        }

        public void RemoveCookie(string cookieKey)
        {
            HttpContext.Current.Response.Cookies.Remove(cookieKey);
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
    }
}
