using System;
using System.Configuration;
using System.Web.Mvc;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    /// <summary>
    /// AuthorizeAttribute class that allows inserting addtional roles from a key in the app settings.
    /// </summary>
    public class AppSettingsAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        ///  Gets or sets the user roles that are authorized to access the controller or action method.
        /// </summary>
        public new string Roles
        {
            get { return base.Roles ?? String.Empty; }
            set
            {
                var sRoles = ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:Roles"]?.ToString();
                if (!String.IsNullOrWhiteSpace(sRoles))
                {
                    base.Roles = value + ',' + sRoles;
                }
                else
                {
                    base.Roles = value;
                }
            }
        }
    }
}
