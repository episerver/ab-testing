using EPiServer.Core;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        /// <summary>
        /// Given the specified Guid, get the content data from cms
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public IContent getContent(Guid guid)
        {
            IContentRepository repo = _serviceLocator.GetInstance<IContentRepository>();
            return repo.Get<IContent>(guid);
        }
    }
}
