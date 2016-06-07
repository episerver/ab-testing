using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(ITestResultHelper), Lifecycle = ServiceInstanceScope.Singleton)]

    public class TestResultHelper : ITestResultHelper
    {
        private IServiceLocator _serviceLocator;
        private IContentRepository _contentRepository;
        private IContent retContent;

        public TestResultHelper()
        {
            _serviceLocator = ServiceLocator.Current;
            _contentRepository = _serviceLocator.GetInstance<IContentRepository>();
        }

        public IContent GetClonedContentFromReference(ContentReference reference)
        {
            retContent = _contentRepository.Get<ContentData>(reference).CreateWritableClone() as IContent;
            return retContent;
        }

        public bool PublishContent(IContent contentToPublish)
        {
            try
            {
                _contentRepository.Save(contentToPublish, DataAccess.SaveAction.Publish);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }
}
