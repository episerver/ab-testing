using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.ServiceLocation;
using EPiServer.Globalization;
using EPiServer.Security;
using EPiServer.Web;
using EPiServer.Web.Routing;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Personalization;

namespace EPiServer.Marketing.Testing.Web.Helpers
{

    [ServiceConfiguration(ServiceType = typeof(ITestingContextHelper))]
    public class TestingContextHelper : ITestingContextHelper
    {
        private readonly IServiceLocator _serviceLocator;
        private IHttpContextHelper _contextHelper;
        private IEpiserverHelper _episerverHelper;

        [ExcludeFromCodeCoverage]
        public TestingContextHelper()
        {
            _serviceLocator = ServiceLocator.Current;
            _contextHelper = new HttpContextHelper();
            _episerverHelper = new EpiserverHelper();
        }

        /// <summary>
        /// For Unit Testing
        /// </summary>
        /// <param name="context"></param>
        /// <param name="mockServiceLocator"></param>
        [ExcludeFromCodeCoverage]
        internal TestingContextHelper(IHttpContextHelper contextHelper, IServiceLocator mockServiceLocator, IEpiserverHelper episerverHelper)
        {
            _contextHelper = contextHelper;
            _serviceLocator = mockServiceLocator;
            _episerverHelper = episerverHelper;
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
                    SkipRequest() ||
                    !Personalize() ||
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
                    SkipRequest() ||
                    !Personalize() ||
                    IsInSystemFolder());
        }

        /// <summary>
        /// Check that personalization is enabled.
        /// </summary>
        /// <returns>true if allowed, else false</returns>
        private bool Personalize()
        {
            var evaluator = _serviceLocator.GetInstance<IAggregatedPersonalizationEvaluator>();
            return evaluator.Personalize();
        }

        /// <summary>
        /// Returns true if the http request is null or should be skipped for various reasons.
        /// </summary>
        /// <returns></returns>
        private bool SkipRequest()
        {
            return !_contextHelper.HasCurrentContext() ||
                !_contextHelper.HasUserAgent() || // MAR 797 - Ignore requests with no user agent specified
                _contextHelper.HasItem(TestHandler.ABTestHandlerSkipFlag);
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
            var isMatchedPage = false;

            if (!(loadedContent is PageData))
                isMatchedPage = true;
            else 
            {
                var content = GetCurrentPage();
                if (content != null)
                    isMatchedPage = (content.ContentLink == loadedContent.ContentLink);
            }

            return isMatchedPage;
        }

        /// <summary>
        /// Converts the current URL to an IContent object
        /// </summary>
        /// <returns></returns>
        public IContent GetCurrentPage()
        {
            // Get the page associate with this request once
            // and store in the request so we can use it later
            try
            {
                var pageHelper = _serviceLocator.GetInstance<IPageRouteHelper>();
                return pageHelper.Page;
            }
            catch { } // sometimes requests dont contain epi pages.

            return null;
        }

        public MarketingTestingContextModel GenerateContextData(IMarketingTest testData)
        {
            var uiHelper = _serviceLocator.GetInstance<IUIHelper>();
            var repo = _serviceLocator.GetInstance<IContentRepository>();
            var publishedVariant = testData.Variants.First(v => v.IsPublished);
            var draftVariant = testData.Variants.First(v => !v.IsPublished);

            //get published version
            var Content = repo.Get<IContent>(testData.OriginalItemId);

            //get content which was published at time of test
            var tempPublishedContentClone = Content.ContentLink.CreateWritableClone();
            int publishedVersion = publishedVariant.ItemVersion;
            tempPublishedContentClone.WorkID = publishedVersion;
            var publishedContent = repo.Get<IContent>(tempPublishedContentClone);

            //get variant (draft) version
            var tempVariantContentClone = Content.ContentLink.CreateWritableClone();
            int variantVersion = draftVariant.ItemVersion;
            tempVariantContentClone.WorkID = variantVersion;
            var draftContent = repo.Get<IContent>(tempVariantContentClone);

            // map the test data into the model using epi icontent and test object 
            var model = new MarketingTestingContextModel();
            decimal publishedVersionAverage = 0;
            decimal draftVersionAverage = 0;

            model.Test = testData;
            model.PublishedVersionName = publishedContent.Name;
            model.DraftVersionContentLink = draftContent.ContentLink.ToString();
            model.DraftVersionName = draftContent.Name;
            model.VisitorPercentage = testData.ParticipationPercentage.ToString();
            model.LatestVersionContentLink = Content.ContentLink.ToString();

            if (testData.KpiInstances.Count == 1)
            {
                var kpiResultType = testData.KpiInstances[0].KpiResultType;
                if (kpiResultType == "KpiFinancialResult")
                {
                    var commerceSettings = _serviceLocator.GetInstance<IKpiManager>().GetCommerceSettings();

                    model.PublishedVersionValuesAverage = publishedVariant.KeyFinancialResults.Count > 0 ? publishedVariant.KeyFinancialResults.Average(x => x.ConvertedTotal).ToString("C", commerceSettings.preferredFormat) : publishedVersionAverage.ToString("C", commerceSettings.preferredFormat);
                    model.DraftVersionValuesAverage = draftVariant.KeyFinancialResults.Count > 0 ? draftVariant.KeyFinancialResults.Average(x => x.ConvertedTotal).ToString("C", commerceSettings.preferredFormat) : draftVersionAverage.ToString("C", commerceSettings.preferredFormat);
                } else if (kpiResultType == "KpiValueResult")
                {
                    model.PublishedVersionValuesAverage = publishedVariant.KeyValueResults.Count > 0 ? publishedVariant.KeyValueResults.Average(x => x.Value).ToString() : "0";
                    model.DraftVersionValuesAverage = draftVariant.KeyValueResults.Count > 0 ? draftVariant.KeyValueResults.Average(x => x.Value).ToString() : "0";
                }
            }


            model.KpiResultType = testData.KpiInstances[0].KpiResultType;

            // Map the version data
            MapVersionData(publishedContent, draftContent, model);

            // Map users publishing rights
            model.UserHasPublishRights = Content.QueryDistinctAccess(AccessLevel.Publish);

            // Map elapsed and remaining days.
            if (testData.State == TestState.Active)
            {
                model.DaysElapsed = Math.Round(DateTime.Now.Subtract(model.Test.StartDate.ToLocalTime()).TotalDays).ToString(CultureInfo.CurrentCulture);
                model.DaysRemaining = Math.Round(model.Test.EndDate.ToLocalTime().Subtract(DateTime.Now).TotalDays).ToString(CultureInfo.CurrentCulture);
            }
            else
            {
                model.DaysElapsed = Math.Round(model.Test.EndDate.ToLocalTime().Subtract(model.Test.StartDate.ToLocalTime()).TotalDays).ToString(CultureInfo.CurrentCulture);
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
        public bool IsInSystemFolder()
        {
            var inSystemFolder = true;

            if (_contextHelper.HasCurrentContext())
            {
                if (_contextHelper.HasItem("InSystemFolder"))
                {
                    inSystemFolder = (bool)_contextHelper.GetCurrentContext().Items["InSystemFolder"];
                }
                else
                {
                    inSystemFolder = _contextHelper.RequestedUrl().ToLower()
                        .Contains(_episerverHelper.GetRootPath().ToLower());

                    _contextHelper.SetItemValue("InSystemFolder", inSystemFolder);
                }
            }

            return inSystemFolder;
        }

        public bool IsHtmlContentType()
        {
             return _contextHelper.HasCurrentContext() ? 
                _contextHelper.GetCurrentContext().Request.AcceptTypes.Contains("text/html") : false;
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
            var publishPreview = _episerverHelper.GetPreviewUrl(publishedVersionData.ContentLink, currentCulture.Name, new VirtualPathArguments() { ContextMode = ContextMode.Preview });
            var draftPreview = _episerverHelper.GetPreviewUrl(draftContent.ContentLink, currentCulture.Name, new VirtualPathArguments() { ContextMode = ContextMode.Preview });

            model.PublishPreviewUrl = publishPreview;
            model.DraftPreviewUrl = draftPreview;
        }
    }
}
