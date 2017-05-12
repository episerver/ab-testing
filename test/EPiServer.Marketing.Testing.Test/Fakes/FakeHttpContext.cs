using System;
using System.IO;
using System.Web;
using System.Web.SessionState;

namespace EPiServer.Marketing.Testing.Test.Fakes
{
    public class FakeHttpContext
    {
        /// <summary>
        /// Creates fake http context
        /// </summary>
        /// <param name="url">Url to create the context for</param>
        /// <returns></returns>
        public static HttpContext FakeContext(string url)
        {
            var uri = new Uri(url);
            var httpRequest = new HttpRequest(string.Empty, uri.ToString(),
                                                uri.Query.TrimStart('?'));
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            var sessionContainer = new HttpSessionStateContainer("id",
                                            new SessionStateItemCollection(),
                                            new HttpStaticObjectsCollection(),
                                            10, true, HttpCookieMode.AutoDetect,
                                            SessionStateMode.InProc, false);

            SessionStateUtility.AddHttpSessionStateToContext(
                                                 httpContext, sessionContainer);

            return httpContext;
        }
    }
}
