using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EPiServer.Marketing.Multivariate.Web.Helpers
{
    public class UIHelper : IUIHelper
    {
        private IServiceLocator _serviceLocator;

        /// <summary>
        /// Default constructor
        /// </summary>
        public UIHelper()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        /// <summary>
        /// unit tests should use this contructor and add needed services to the service locator as needed
        /// </summary>
        /// <param name="locator"></param>
        internal UIHelper(IServiceLocator locator)
        {
            _serviceLocator = locator;
        }

        public string getConfigurationURL()
        {
            //Build EPiServer URL to the configuration page
            var uri = new Uri(HttpContext.Current.Request.Url.ToString());
            var port = string.Empty;
            if (uri.Port != 80)
                port = ":" + uri.Port;

            var requested = uri.Scheme + "://" + uri.Host + port;
            string settingsUrl = GetConnectorSettingsUrlString();
            return string.Format("{0}{1}", requested, settingsUrl);
        }

        private string GetConnectorSettingsUrlString()
        {
            // out: http://{DOMAIN}/{PROTECTED_PATH}/CMS/Admin/
            var baseUrl = EPiServer.UriSupport.ResolveUrlFromUIBySettings("Admin/");

            // out: /{PROTECTED_PATH}/{MODULE_NAME}/Views/Admin/Settings.aspx
            var targetResourcePath = Paths.ToResource(typeof(UIHelper), "MultivariateConfiguration.aspx");

            // out: http://{DOMAIN}/{PROTECTED_PATH}/CMS/Admin/?customdefaultpage=/{PROTECTED_PATH}/{MODULE_NAME}/Views/Admin/Settings.aspx
            return EPiServer.UriSupport.AddQueryString(baseUrl, "customdefaultpage", targetResourcePath);
        }

        /// <summary>
        /// Given the specified Guid, get the content data from cms
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>The icontent object if found, if not found returns a BasicContent instance with name set to ContentNotFound</returns>
        public IContent getContent(Guid guid)
        {
            IContentRepository repo = _serviceLocator.GetInstance<IContentRepository>();
            try
            {
                return repo.Get<IContent>(guid);
            } catch
            {
                return new BasicContent() { Name = "ContentNotFound" };
            }
        }
    }
}
