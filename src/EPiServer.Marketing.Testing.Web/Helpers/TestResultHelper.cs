using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Core;
using EPiServer.DataAccess;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Initializers;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(ITestResultHelper), Lifecycle = ServiceInstanceScope.Singleton)]

    public class TestResultHelper : ITestResultHelper
    {
        private IServiceLocator _serviceLocator;
        private IContentRepository _contentRepository;

        public TestResultHelper()
        {
            _serviceLocator = ServiceLocator.Current;
            _contentRepository = _serviceLocator.GetInstance<IContentRepository>();
        }

        public IContent GetClonedContentFromReference(ContentReference reference)
        {
            return _contentRepository.Get<ContentData>(reference).CreateWritableClone() as IContent;
        }

        public ContentReference PublishContent(IContent contentToPublish)
        {
            PublishContentEventListener.addPublishingContent(contentToPublish);
            return _contentRepository.Save(contentToPublish, DataAccess.SaveAction.Publish);
        }
    }
}
