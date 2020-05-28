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
    /// AB Authorization class that allows inclusion of roles and users from app settings.
    /// </summary>
    public class ABAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// The roles to be authorized against.
        /// </summary>
        protected new List<string> Roles = new List<string>();

        /// <summary>
        /// The users to be authorized.
        /// </summary>
        protected new List<string> Users = new List<string>();

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="roles">Default roles, can be empty</param>
        /// <param name="users">Default users, can be empty</param>
        public ABAuthorizeAttribute(string roles = "", string users = "")
        {
            Roles.AddRange(SplitString(roles));
            Roles.AddRange(SplitString(ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:Roles"]?.ToString()));
            Users.AddRange(SplitString(users));
            Users.AddRange(SplitString(ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:Users"]?.ToString()));
        }

        /// <summary>
        /// Overrridden to use the list of users and roles specified in the app settings or in the constructor.
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
            if (user.Identity.IsAuthenticated)
            {
                if (Roles.Any(user.IsInRole) || Users.Contains(user.Identity.Name, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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