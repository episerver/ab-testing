using System.Diagnostics.CodeAnalysis;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Initializers;
using EPiServer.Security;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(ITestResultHelper), Lifecycle = ServiceInstanceScope.Singleton)]
    [ExcludeFromCodeCoverage]
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
            return _contentRepository.Save(contentToPublish, DataAccess.SaveAction.Publish, AccessLevel.Publish);
        }
    }
}
