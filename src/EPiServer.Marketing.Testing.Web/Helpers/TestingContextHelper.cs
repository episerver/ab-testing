using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.ServiceLocation;
using EPiServer.Globalization;
using EPiServer.Security;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public class TestingContextHelper : ITestingContextHelper
    {
        private IServiceLocator _serviceLocator;
        
        public TestingContextHelper()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        /// <summary>
        /// For Unit Testing
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mockServiceLocator"></param>
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
        /// Checks the current loaded content with the requested page.
        /// Page Data content is loaded even if not the requested page, wheras Block Data is only
        /// loaded when included in the page.
        /// </summary>
        /// <param name="loadedContent"></param>
        /// <returns> True if not pagedata or if content is pagedata and
        ///  matches requested page</returns>
        public bool IsRequestedContent(IContent loadedContent)
        {

            if (loadedContent is PageData)
            {
                if (!(GetCurrentPageFromUrl().ContentLink == loadedContent.ContentLink))
                    return false;
            }
            return true;
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
            var uiHelper = _serviceLocator.GetInstance<IUIHelper>();
            var repo = _serviceLocator.GetInstance<IContentRepository>();

            //get published version
            var publishedContent = repo.Get<IContent>(testData.OriginalItemId);

            //get variant (draft) version
            var tempContentClone = publishedContent.ContentLink.CreateWritableClone();
            int variantVersion = testData.Variants.First(x => !x.ItemVersion.Equals(publishedContent.ContentLink.ID)).ItemVersion;
            tempContentClone.WorkID = variantVersion;
            var draftContent = repo.Get<IContent>(tempContentClone);

            // map the test data into the model using epi icontent and test object 
            var model = new MarketingTestingContextModel();
            model.Test = testData;
            model.PublishedVersionName = publishedContent.Name;
            model.DraftVersionContentLink = draftContent.ContentLink.ToString();
            model.DraftVersionName = draftContent.Name;
            model.VisitorPercentage = testData.ParticipationPercentage.ToString();

            // Map the version data
            MapVersionData(publishedContent, draftContent, model);

            // Map users publishing rights
            model.UserHasPublishRights = publishedContent.QueryDistinctAccess(AccessLevel.Publish);
            
            //Test Details may be viewed before the test has started.   
            //Check state and set the contextmodel days elapsed and days remaining to appropriate strings
            //Text message if Inactive, Remaining Days if active, and adjusted days for done and archived.
            //Days Elapsed will be parsed and displayed using episervers friendly datetime method on the client side.
            if (testData.State == TestState.Active)
            {
                model.DaysElapsed = Math.Round(DateTime.Now.Subtract(DateTime.Parse(model.Test.StartDate.ToString())).TotalDays).ToString(CultureInfo.CurrentCulture); ;
                model.DaysRemaining = Math.Round(DateTime.Parse(model.Test.EndDate.ToString()).Subtract(DateTime.Now).TotalDays).ToString(CultureInfo.CurrentCulture);
            }
            else if (testData.State == TestState.Inactive)
            {
                model.DaysElapsed = "Test has not been started";
                model.DaysRemaining = "Test has not been started";
            }
            else
            {
                model.DaysElapsed = Math.Round(DateTime.Parse(model.Test.EndDate.ToString()).Subtract(DateTime.Parse(model.Test.StartDate.ToString())).TotalDays).ToString(CultureInfo.CurrentCulture);
                model.DaysRemaining = "0";
            }

            //retrieve conversion content from kpis
            //convert conversion content link to anchor link
            var kpi = testData.KpiInstances[0] as ContentComparatorKPI;
            if (kpi != null)
            {
                var conversionContent = repo.Get<IContent>(kpi.ContentGuid);
               
                model.ConversionLink = uiHelper.getEpiUrlFromLink(conversionContent.ContentLink);
                model.ConversionContentName = conversionContent.Name;
            }

            // Calculate total participation count
            foreach (var variant in testData.Variants)
            {
                model.TotalParticipantCount += variant.Views;
            }

            return model;
            }

        /// <summary>
        /// Map IContent version data into the model
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="draftContent"></param>
        /// <param name="model"></param>
        private void MapVersionData(IContent publishedContent, IContent draftContent, MarketingTestingContextModel model)
        {
            var versionRepo = _serviceLocator.GetInstance<IContentVersionRepository>();
            var publishedVersionData = versionRepo.LoadPublished(publishedContent.ContentLink,
                ContentLanguage.PreferredCulture.Name);
            var draftVersionData = versionRepo.Load(draftContent.ContentLink);

            //set published and draft version info
            model.PublishedVersionPublishedBy = string.IsNullOrEmpty(publishedVersionData.StatusChangedBy) ? publishedVersionData.SavedBy : publishedVersionData.StatusChangedBy;
            model.PublishedVersionPublishedDate = publishedVersionData.Saved.ToString(CultureInfo.CurrentCulture);
            model.PublishedVersionContentLink = publishedVersionData.ContentLink.ToString();

            model.DraftVersionChangedBy = string.IsNullOrEmpty(draftVersionData.StatusChangedBy) ? draftVersionData.SavedBy : draftVersionData.StatusChangedBy;
            model.DraftVersionChangedDate = draftVersionData.Saved.ToString(CultureInfo.CurrentCulture);
        }
    }
}
