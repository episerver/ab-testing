using System.Configuration;
using System.Web.Mvc;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    /// <summary>
    /// AB Authorization class that allows inclusion of groups from app settings in web app.
    /// </summary>
    public class ABAuthorizeAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public ABAuthorizeAttribute()
        {
            var addRoles = ConfigurationManager.AppSettings["EPiServer:Marketing:Testing:Roles"]?.ToString();
            this.Roles = addRoles ?? this.Roles;

        }

        public override void OnAuthorization(AuthorizationContext actionContext)
        {
            base.OnAuthorization(actionContext);
        }
  
    }
}