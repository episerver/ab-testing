using EPiServer.Core.Transfer.Internal;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    /// <summary>
    /// AB Authorization class that allows inclusion of groups from app settings in web app.
    /// </summary>
    public class ABAuthorizeAttribute : AuthorizeAttribute
    {
        protected List<string> DefaultRoles = new List<string>();

        /// <summary>
        /// default constructor
        /// </summary>
        /// <param name="roles">Default roles, can be empty</param>
        public ABAuthorizeAttribute(string Roles = "")
        {
            DefaultRoles.AddRange( SplitString(Roles).ToList() );

//            var addRoles = ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:Roles"]?.ToString();
//            this.Roles = addRoles ?? this.Roles;

        }
        // This method must be thread-safe since it is called by the thread-safe OnCacheAuthorization() method.
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

            // this works
            //var roles = SplitString( ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:Roles"]?.ToString() );
            //if (roles.Length > 0 && !roles.Any(user.IsInRole))
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