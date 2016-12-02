using System;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Evaluator
{
    public class ABTestLockEvaluator : IContentLockEvaluator
    {
        private IMarketingTestingWebRepository _webRepo;

        public ABTestLockEvaluator()
        {
            _webRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
        }

        public ContentLock IsLocked(ContentReference contentLink)
        {
            var contentRepo = ServiceLocator.Current.GetInstance<IContentRepository>();
            var content = contentRepo.Get<IContent>(contentLink);

            if (content == null)
                return null;

            var test = _webRepo.GetActiveTestForContent(content.ContentGuid);

            if (test == null || test.Id == null || test.Id == Guid.Empty)
                return null;

            return new ContentLock(contentLink, test.Owner, "ActiveABTestLock", test.CreatedDate);
        }
    }
}
