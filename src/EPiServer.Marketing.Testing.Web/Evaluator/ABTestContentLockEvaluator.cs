using System;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Evaluator
{
    public class ABTestLockEvaluator : IContentLockEvaluator
    {
        private IMarketingTestingWebRepository _webRepo;
        private IContentRepository _contentRepo;
        internal string _abLockId = "ActiveABTestLock";

        public ABTestLockEvaluator()
        {
            _webRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _contentRepo = ServiceLocator.Current.GetInstance<IContentRepository>();
        }

        internal ABTestLockEvaluator(IMarketingTestingWebRepository theWebRepo, IContentRepository theContentRepo)
        {
            _webRepo = theWebRepo;
            _contentRepo = theContentRepo;
        }

        public ContentLock IsLocked(ContentReference contentLink)
        {
            var content = _contentRepo.Get<IContent>(contentLink);

            if (content == null)
                return null;

            var test = _webRepo.GetActiveTestForContent(content.ContentGuid);

            if (test == null || test.Id == null || test.Id == Guid.Empty)
                return null;

            return new ContentLock(contentLink, test.Owner, _abLockId, test.CreatedDate);
        }
    }
}
