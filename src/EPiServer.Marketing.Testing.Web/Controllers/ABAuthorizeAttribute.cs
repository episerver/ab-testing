using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    /// <summary>
    /// AB Authorization class that allows inclusion of groups from app settings in web app.
    /// </summary>
    public class ABAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// The roles to be authorized against.
        /// </summary>
        protected List<string> DefaultRoles = new List<string>();

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="roles">Default roles, can be empty</param>
        public ABAuthorizeAttribute(string roles = "")
        {
            DefaultRoles.AddRange(SplitString(roles));
            DefaultRoles.AddRange(SplitString(ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:Roles"]?.ToString()));
        }

        /// <summary>
        /// Overrridden to use the list of users and roles specified in the app settings or at construction.
        /// </summary>
        /// <param name="httpContext">The HttpContext.</param>
        /// <returns></returns>
        /// <remarks>This method must be thread-safe since it is called by the thread-safe OnCacheAuthorization() method.</remarks> 
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException("httpContext");
            }

            IPrincipal user = httpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                return false;
            }

            //if (_usersSplit.Length > 0 && !_usersSplit.Contains(user.Identity.Name, StringComparer.OrdinalIgnoreCase))
            //{
            //    return false;
            //}

            if (!DefaultRoles.Any(user.IsInRole))
            {
                return false;
            }

            return true;
        }

        internal static string[] SplitString(string original)
        {
            if (String.IsNullOrEmpty(original))
            {
                return new string[0];
            }

            var split = from piece in original.Split(',')
                        let trimmed = piece.Trim()
                        where !String.IsNullOrEmpty(trimmed)
                        select trimmed;
            return split.ToArray();
        }
    }
}