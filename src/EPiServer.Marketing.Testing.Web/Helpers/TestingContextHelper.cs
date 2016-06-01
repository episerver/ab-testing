
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.ServiceLocation;
using EPiServer.Web.Mvc.Html;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public class TestingContextHelper : ITestingContextHelper
    {
        private IContentRepository _contentRepository;
        private IContentVersionRepository _contentVersionRepository;
        private IUIHelper _uiHelper;
        private IServiceLocator _serviceLocator;
        
        public TestingContextHelper()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        /// <summary>
        /// For Unit Testing
        /// </summary>
        /// <param name="context"></param>
        [ExcludeFromCodeCoverage]
        internal TestingContextHelper(HttpContext context, IServiceLocator mockServiceLocator)
        {
            HttpContext.Current = context;
            _serviceLocator = mockServiceLocator;
        }



        /// <summary>
        /// Evaluates current URL to determine if page is in a system folder context (e.g Edit, or Preview)
        /// </summary>
        /// <returns></returns>
        public bool IsInSystemFolder()
        {
            var currentContext = HttpContext.Current;
            if (currentContext == null)
            {
                return true;
            }

            return currentContext.Request.RawUrl.ToLower()
                .Contains(EPiServer.Shell.Paths.ProtectedRootPath.ToLower());
        }

        /// <summary>
        /// Compares the current content with the requested content
        /// </summary>
        /// <param name="requestedContent"></param>
        /// <param name="loadedContent"></param>
        /// <returns></returns>
        public bool IsRequestedContent(IContent requestedContent, IContent loadedContent)
        {
            return requestedContent.ContentLink == loadedContent.ContentLink;
        }

        /// <summary>
        /// Converts the current URL to an IContent object
        /// </summary>
        /// <returns></returns>
        public IContent GetCurrentPageFromUrl()
        {
            return HttpContext.Current.Items["CurrentPage"] as IContent;
        }

        public MarketingTestingContextModel GenerateContextData(IMarketingTest testData)
        {
            _contentRepository = _serviceLocator.GetInstance<IContentRepository>();
            _contentVersionRepository = _serviceLocator.GetInstance<IContentVersionRepository>();
            _uiHelper = _serviceLocator.GetInstance<IUIHelper>();

            var marketingTestingContextModel = new MarketingTestingContextModel();

            //set contextmodel IMarketingTest data
            marketingTestingContextModel.Test = testData;

            //convert test data StartDate to local time;
            marketingTestingContextModel.Test.StartDate = marketingTestingContextModel.Test.StartDate.ToLocalTime();

            //get published version
            var publishedContentPageData = _contentRepository.Get<PageData>(testData.OriginalItemId);
            var publishedVersionData = _contentVersionRepository.LoadPublished(publishedContentPageData.ContentLink, publishedContentPageData.LanguageBranch);

            //set required contextmodel published version data
            marketingTestingContextModel.PublishedVersionContentLink = publishedContentPageData.ContentLink.ToString();
            marketingTestingContextModel.PublishedVersionName = publishedContentPageData.Name;
            marketingTestingContextModel.PublishedVersionPublishedBy = string.IsNullOrEmpty(publishedVersionData.StatusChangedBy) ? publishedVersionData.SavedBy : publishedVersionData.StatusChangedBy;
            marketingTestingContextModel.PublishedVersionPublishedDate = publishedContentPageData.StartPublish.ToString(CultureInfo.CurrentCulture);

            //get variant version
            var tempContentClone = publishedContentPageData.ContentLink.CreateWritableClone();
            int variantVersion = testData.Variants.First(x => !x.ItemVersion.Equals(publishedContentPageData.ContentLink.ID)).ItemVersion;
            tempContentClone.WorkID = variantVersion;
            var draftContent = _contentRepository.Get<PageData>(tempContentClone);

            //set required contextmodel variant version data
            marketingTestingContextModel.DraftVersionContentLink = draftContent.ContentLink.ToString();
            marketingTestingContextModel.DraftVersionName = draftContent.PageName;
            marketingTestingContextModel.DraftVersionChangedBy = draftContent.ChangedBy;
            marketingTestingContextModel.DraftVersionChangedDate = draftContent.Saved.ToString(CultureInfo.CurrentCulture);

            //Test Details may be viewed before the test has started.   
            //Check state and set the contextmodel days elapsed and days remaining to appropriate strings
            //Text message if Inactive, Remaining Days if active.   Days Elapsed will be parsed and displayed using
            //episervers friendly datetime method on the client side.
            if (testData.State == TestState.Active)
            {
                marketingTestingContextModel.DaysElapsed = Math.Round(DateTime.Now.Subtract(DateTime.Parse(marketingTestingContextModel.Test.StartDate.ToString())).TotalDays).ToString(CultureInfo.CurrentCulture); ;
                marketingTestingContextModel.DaysRemaining = Math.Round(DateTime.Parse(marketingTestingContextModel.Test.EndDate.ToString()).Subtract(DateTime.Now).TotalDays).ToString(CultureInfo.CurrentCulture);
            }
            else if (testData.State == TestState.Inactive)
            {
                marketingTestingContextModel.DaysElapsed = "Test has not been started";
                marketingTestingContextModel.DaysRemaining = "Test has not been started";
            }

            //retrieve conversion content from kpis
            //convert conversion content link to anchor link
            var kpi = testData.KpiInstances[0] as ContentComparatorKPI;
            if (kpi != null)
            {
                var conversionContent = _contentRepository.Get<IContent>(kpi.ContentGuid);
               
                marketingTestingContextModel.ConversionLink = _uiHelper.getEpiUrlFromLink(conversionContent.ContentLink);
                marketingTestingContextModel.ConversionContentName = conversionContent.Name;
            }

            //set test details visitor precentage and total visitors
            marketingTestingContextModel.VisitorPercentage = testData.ParticipationPercentage.ToString();

            foreach (var variant in testData.Variants)
            {
                marketingTestingContextModel.TotalParticipantCount += variant.Views;
            }

            return marketingTestingContextModel;
        }
    }
}
