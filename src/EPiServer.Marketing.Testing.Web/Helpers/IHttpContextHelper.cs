using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EPiServer.Marketing.Testing.Web.Helpers
{

    public interface IHttpContextHelper
    {
        /// <summary>
        /// Searches the HttpContexts items collection for the specified item ID.
        /// </summary>
        /// <param name="itemId">ID of the item in the Items collection.</param>
        /// <returns>if the item was found or not</returns>
        bool HasItem(string itemId);

        /// <summary>
        /// Returns a request param value, if it exists.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        string GetRequestParam(string itemId);

        /// <summary>
        /// Sets the value of the specified item in the HttpContexts items collection.
        /// </summary>
        /// <param name="itemId">ID of the item in the collection.</param>
        /// <param name="value">Value to set the item to.</param>
        void SetItemValue(string itemId, object value);

        /// <summary>
        /// Removes an item from the HttpContexts Items collection.
        /// </summary>
        /// <param name="itemId">Key of the item to remove.</param>
        void RemoveItem(string itemId);

        /// <summary>
        /// Searches the HttpContexts cookie collection to determin if the cookie key passed in is present.
        /// </summary>
        /// <param name="cookieKey">Cookie key value to search the collection with.</param>
        /// <returns>If the item was found or not.</returns>
        bool HasCookie(string cookieKey);

        /// <summary>
        /// Reads the cookie value from the cookie collection in the HttpContext.
        /// </summary>
        /// <param name="cookieKey">cookie key value to search the collection with</param>
        /// <returns>the value of the cookie in the collection</returns>
        string GetCookieValue(string cookieKey);

        /// <summary>
        /// /// Returns the cookie from the HttpContext Response objects cookie collection.
        /// </summary>
        /// <param name="cookieKey">Cookie key value to search the collection with.</param>
        /// <returns>The cookie found in the cookie collection.</returns>
        HttpCookie GetResponseCookie(string cookieKey);

        /// <summary>
        /// Returns the cookie from the HttpContext Request objects cookie collection.
        /// </summary>
        /// <param name="cookieKey">Cookie key value to search the collection with.</param>
        /// <returns>The cookie found in the cookie collection.</returns>
        HttpCookie GetRequestCookie(string cookieKey);

        /// <summary>
        /// Gets all the keys from the HttpContext Response objects cookie collection.
        /// </summary>
        /// <returns>All the cookie keys.</returns>
        string[] GetResponseCookieKeys();

        /// <summary>
        /// Gets all the cookie keys from the HttpContext Request objects cookie collection.
        /// </summary>
        /// <returns>All the cookie keys.</returns>
        string[] GetRequestCookieKeys();

        /// <summary>
        /// Removes the specified cookie from the cookie collection in the HttpContext.
        /// </summary>
        /// <param name="cookieKey">Identifier for the cookie.</param>
        void RemoveCookie(string cookieKey);

        /// <summary>
        /// Adds the cookie to the HttpContext's cookie collection.
        /// </summary>
        /// <param name="cookie">The cookie to add</param>
        void AddCookie(HttpCookie cookie);

        /// <summary>
        /// Indicates whether the HttpContext response is in a writable state.
        /// </summary>
        /// <returns>The writable state of the HttpContext.</returns>
        bool CanWriteToResponse();

        /// <summary>
        /// Returns the HttpContext response filter stream.
        /// </summary>
        /// <returns></returns>
        Stream GetResponseFilter();

        /// <summary>
        /// Used for setting the HttpContexts response filter to the stream specified.
        /// </summary>
        /// <param name="stream">The stream to use for the response filter.</param>
        void SetResponseFilter(Stream stream);

        /// <summary>
        /// Checks the context to see if there is a current context for the HttpContext object.
        /// </summary>
        /// <returns>If the current context exists.</returns>
        bool HasCurrentContext();

        /// <summary>
        /// Checks the HttpContext for the existance of a UserAgent.
        /// </summary>
        /// <returns>If the UserAgent was found.</returns>
        bool HasUserAgent();

        /// <summary>
        /// Returns the HttpContext's requested raw url.
        /// </summary>
        /// <returns>The url string.</returns>
        string RequestedUrl();

        HttpContext GetCurrentContext();

    }
}
