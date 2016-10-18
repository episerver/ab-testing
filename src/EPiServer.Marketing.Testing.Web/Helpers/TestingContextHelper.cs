using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.ServiceLocation;
using EPiServer.Globalization;
using EPiServer.Security;
using EPiServer.Web;
using EPiServer.Web.Routing;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    #region UnitTestWorkaround
    internal interface IPreviewUrlBuilder
    {
        string GetPreviewUrl(ContentReference cr, string language, VirtualPathArguments args);
    }
    [ExcludeFromCodeCoverage]
    internal class PreviewUrlBuilder : IPreviewUrlBuilder
    {
        public string GetPreviewUrl(ContentReference cr, string language, VirtualPathArguments args)
        {
            return UrlResolver.Current.GetUrl(cr, language, args);
        }
    }
    #endregion UnitTestWorkaround

    public class TestingContextHelper : ITestingContextHelper
    {
        private readonly IServiceLocator _serviceLocator;
        private IPreviewUrlBuilder _previewUrlBuilder;

        public TestingContextHelper()
        {
            _serviceLocator = ServiceLocator.Current;
            _previewUrlBuilder = new PreviewUrlBuilder();
        }

        /// <summary>
        /// For Unit Testing
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mockServiceLocator"></param>
        [ExcludeFromCodeCoverage]
        internal TestingContextHelper(HttpContext context, IServiceLocator mockServiceLocator, IPreviewUrlBuilder mockUrlBuilder)
        {
            HttpContext.Current = context;
            _serviceLocator = mockServiceLocator;
            _previewUrlBuilder = mockUrlBuilder;
        }

        /// <summary>
        /// Evaluates a set of conditions which would preclue a test from swapping content
        /// * Specific to loading regular content
        /// </summary>
        /// <returns></returns>
        public bool SwapDisabled(EventArgs e)
        {
            //currently, our only restriction is user being logged into a system folder (e.g edit).
            //Other conditions have been brought up such as permissions, ip restrictions etc
            //which can be evaluated together here or individually.
            ContentEventArgs ea = e as ContentEventArgs;
            return ( (ea != null && ea.Content == null) || // if e is a contenteventargs make sure we have content.
                    HttpContext.Current == null ||
                    HttpContext.Current.Items.Contains(TestHandler.ABTestHandlerSkipFlag) ||
                    IsInSystemFolder());
        }

        /// <summary>
        /// Evaluates a set of conditions which would preclue a test from swapping content
        /// * Specific to loading children
        /// </summary>
        /// <returns></returns>
        public bool SwapDisabled(ChildrenEventArgs e)
        {
            //currently, our only restriction is user being logged into a system folder (e.g edit).
            //Other conditions have been brought up such as permissions, ip restrictions etc
            //which can be evaluated together here or individually.
            return (e.ContentLink == null ||
                    e.ChildrenItems == null ||
                    HttpContext.Current == null ||
                    HttpContext.Current.Items.Contains(TestHandler.ABTestHandlerSkipFlag) ||
                    IsInSystemFolder());
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
            var content = GetCurrentPageFromUrl();
            var isMatchedPage = false;
            if (content != null)
            {
                isMatchedPage = !(loadedContent is PageData) || (content.ContentLink == loadedContent.ContentLink);
            }
            else if (!(loadedContent is PageData))
            {
                isMatchedPage = true;
            }

            return isMatchedPage;
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
            var Content = repo.Get<IContent>(testData.OriginalItemId);

            //get content which was published at time of test
            var tempPublishedContentClone = Content.ContentLink.CreateWritableClone();
            int publishedVersion = testData.Variants.First(x => x.IsPublished).ItemVersion;
            tempPublishedContentClone.WorkID = publishedVersion;
            var publishedContent = repo.Get<IContent>(tempPublishedContentClone);

            //get variant (draft) version
            var tempVariantContentClone = Content.ContentLink.CreateWritableClone();
            int variantVersion = testData.Variants.First(x => !x.IsPublished).ItemVersion;
            tempVariantContentClone.WorkID = variantVersion;
            var draftContent = repo.Get<IContent>(tempVariantContentClone);

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
            model.UserHasPublishRights = Content.QueryDistinctAccess(AccessLevel.Publish);

            // Map elapsed and remaining days.
            if (testData.State == TestState.Active)
            {
                model.DaysElapsed = Math.Round(DateTime.Now.Subtract(DateTime.Parse(model.Test.StartDate.ToString())).TotalDays).ToString(CultureInfo.CurrentCulture); ;
                model.DaysRemaining = Math.Round(DateTime.Parse(model.Test.EndDate.ToString()).Subtract(DateTime.Now).TotalDays).ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                model.DaysElapsed = Math.Round(DateTime.Parse(model.Test.EndDate.ToString()).Subtract(DateTime.Parse(model.Test.StartDate.ToString())).TotalDays).ToString(CultureInfo.CurrentCulture);
                model.DaysRemaining = "0";
            }

           // Calculate total participation count
            foreach (var variant in testData.Variants)
            {
                model.TotalParticipantCount += variant.Views;
            }

            return model;
        }

        /// <summary>
        /// Evaluates current URL to determine if page is in a system folder context (e.g Edit, or Preview)
        /// </summary>
        /// <returns></returns>
        internal bool IsInSystemFolder()
        {
            var inSystemFolder = true;

            if (HttpContext.Current != null)
            {
                inSystemFolder = HttpContext.Current.Request.RawUrl.ToLower()
                    .Contains(Shell.Paths.ProtectedRootPath.ToLower());
            }

            return inSystemFolder;
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
            var publishedVersionData = versionRepo.Load(publishedContent.ContentLink);
            var draftVersionData = versionRepo.Load(draftContent.ContentLink);

            //set published and draft version info
            model.PublishedVersionPublishedBy = string.IsNullOrEmpty(publishedVersionData.StatusChangedBy) ? publishedVersionData.SavedBy : publishedVersionData.StatusChangedBy;
            model.PublishedVersionPublishedDate = publishedVersionData.Saved.ToString("o").ToString(CultureInfo.CurrentCulture); // In the view details.js, the function datetime.toUserFriendlyString() expects the input date to be in round - trip date / time pattern "o" (example: "2016-10-12T05:37:24Z") to convert it correctly for display purpose. Format prior to conversion was "10/12/2016 5:33:29 AM".

            model.PublishedVersionContentLink = publishedVersionData.ContentLink.ToString();

            model.DraftVersionChangedBy = string.IsNullOrEmpty(draftVersionData.StatusChangedBy) ? draftVersionData.SavedBy : draftVersionData.StatusChangedBy;
            model.DraftVersionChangedDate = draftVersionData.Saved.ToString("o").ToString(CultureInfo.CurrentCulture); // In the view details.js, the function datetime.toUserFriendlyString() expects the input date to be in round - trip date / time pattern "o" (example: "2016-10-12T05:37:24Z") to convert it correctly for display purpose. Format prior to conversion was "10/12/2016 5:33:29 AM".

            //Set previewUrl's from version data
            var currentCulture = ContentLanguage.PreferredCulture;
            var publishPreview = _previewUrlBuilder.GetPreviewUrl(publishedVersionData.ContentLink, currentCulture.Name, new VirtualPathArguments() { ContextMode = ContextMode.Preview });
            var draftPreview = _previewUrlBuilder.GetPreviewUrl(draftContent.ContentLink, currentCulture.Name, new VirtualPathArguments() { ContextMode = ContextMode.Preview });

            model.PublishPreviewUrl = publishPreview;
            model.DraftPreviewUrl = draftPreview;
        }
    }
}
