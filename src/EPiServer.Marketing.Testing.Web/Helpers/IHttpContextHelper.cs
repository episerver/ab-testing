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
        /// Searches the HttpContexts items collection for the specified item Id
        /// </summary>
        /// <param name="itemId">Id of the item in the Items collection</param>
        /// <returns>if the item was found or not</returns>
        bool HasItem(string itemId);

        /// <summary>
        /// Sets the value of the specified item in the HttpContexts items collection
        /// </summary>
        /// <param name="itemId">id of the item in the collection</param>
        /// <param name="value">value to set the item to</param>
        void SetItemValue(string itemId, object value);

        /// <summary>
        /// Searches the HttpContexts cookie collection to determin if the cookie key passed in is present
        /// </summary>
        /// <param name="cookieKey">cookie key value to search the collection with</param>
        /// <returns>if the item was found or not</returns>
        bool HasCookie(string cookieKey);

        /// <summary>
        /// Reads the cookie value from the cookie collection in the HttpContext
        /// </summary>
        /// <param name="cookieKey">cookie key to read from</param>
        /// <returns></returns>
        string GetCookieValue(string cookieKey);

        /// <summary>
        /// Removes the specified cookie from the cookie collection in the HttpContext
        /// </summary>
        /// <param name="cookieKey">identifier for the cookie</param>
        void RemoveCookie(string cookieKey);

        /// <summary>
        /// Adds the cookie to the HttpContext's cookie collection
        /// </summary>
        /// <param name="cookie">the cookie to add</param>
        void AddCookie(HttpCookie cookie);

        /// <summary>
        /// Indicates whether the HttpContext response is in a writable state
        /// </summary>
        /// <returns>the writable state of the HttpContext</returns>
        bool CanWriteToResponse();

        /// <summary>
        /// Returns the HttpContext response filter stream
        /// </summary>
        /// <returns></returns>
        Stream GetResponseFilter();

        /// <summary>
        /// Used for setting the HttpContexts response filter to the stream specified
        /// </summary>
        /// <param name="stream">the stream to use for the response filter</param>
        void SetResponseFilter(Stream stream);
    }
}
