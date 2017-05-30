using System;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Helpers;

namespace EPiServer.Marketing.Testing.Web.Evaluator
{
    public class ABTestLockEvaluator : IContentLockEvaluator
    {
        private IMarketingTestingWebRepository _webRepo;
        private IContentRepository _contentRepo;
        private IEpiserverHelper _episerverHelper;
        internal string _abLockId = "ActiveABTestLock";

        [ExcludeFromCodeCoverage]
        public ABTestLockEvaluator()
        {
            _webRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _contentRepo = ServiceLocator.Current.GetInstance<IContentRepository>();
            _episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
        }

        internal ABTestLockEvaluator(IMarketingTestingWebRepository theWebRepo, IContentRepository theContentRepo, IEpiserverHelper theEpiHelper)
        {
            _webRepo = theWebRepo;
            _contentRepo = theContentRepo;
            _episerverHelper = theEpiHelper;
        }

        public ContentLock IsLocked(ContentReference contentLink)
        {
            var content = _contentRepo.Get<IContent>(contentLink);

            if (content == null)
                return null;

            var test = _webRepo.GetActiveTestForContent(content.ContentGuid, _episerverHelper.GetContentCultureinfo());

            if (test == null || test.Id == null || test.Id == Guid.Empty)
                return null;

            return new ContentLock(contentLink, test.Owner, _abLockId, test.CreatedDate);
        }
    }
}
