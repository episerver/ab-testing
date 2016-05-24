using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Rest;
using EPiServer.Web.Mvc.Html;


namespace EPiServer.Marketing.Testing.Web.Context
{
    [ServiceConfiguration(typeof(IUriContextResolver))]
    public class MarketingTestingContextResolver : IUriContextResolver
    {
        public static readonly string UriPrefix = "epi.marketing.testing";
        private readonly IMarketingTestingWebRepository _marketingTestRepository;
        private readonly IContentRepository _contentRepository;
        private readonly IContentVersionRepository _contentVersionRepository;

        [ExcludeFromCodeCoverage]
        public MarketingTestingContextResolver()
        {
            var serviceLocator = ServiceLocator.Current;
            _marketingTestRepository = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            _contentRepository = serviceLocator.GetInstance<IContentRepository>();
            _contentVersionRepository = serviceLocator.GetInstance<IContentVersionRepository>();
        }

        internal MarketingTestingContextResolver(IServiceLocator serviceLocator)
        {
            _marketingTestRepository = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            _contentRepository = serviceLocator.GetInstance<IContentRepository>();
            _contentVersionRepository = serviceLocator.GetInstance<IContentVersionRepository>();
        }

        [ExcludeFromCodeCoverage]
        public string Name
        {
            get { return UriPrefix; }
        }

        public bool TryResolveUri(Uri uri, out ClientContextBase instance)
        {
            Guid id;
            IMarketingTest marketingTest = new ABTest();

            instance = null;

            //check url to resolve for actionable segments
            //Segment 1 = guid and guid type
            //Segment 2 = View to render
            if (uri.Segments.Length <= 2 || (!uri.Segments[1].Contains("testid") &&
                                             !uri.Segments[1].Contains("contentid")))
            {
                return false;
            }

            //check url for properly formatted property string
            if (uri.Segments[1].IndexOf('=') == -1)
            {
                return false;
            }

            //check url for proper guid
            if (!Guid.TryParse(uri.Segments[1].TrimEnd('/').Split('=')[1], out id))
            {
                return false;
            }

            //if url is properly formatted with the correct data
            //get back the test based on the guid type given
            var idType = uri.Segments[1].Split('=')[0];

            switch (idType)
            {
                case "testid":
                    marketingTest = _marketingTestRepository.GetTestById(id);
                    break;
                case "contentid":
                    marketingTest = _marketingTestRepository.GetActiveTestForContent(id);
                    break;
            }

            //check that a test was found
            if (marketingTest == null)
            {
                return false;
            }

            //set required view to render
            string targetView = uri.Segments[2];

            // Create context instance
            instance = new MarketingTestContext
            {
                Uri = uri,
                RequestedUri = uri,
                VersionAgnosticUri = uri,
                Name = marketingTest.Title,
                DataType = typeof(MarketingTestingContextModel).FullName.ToLowerInvariant(),
                Data = GenerateContextData(marketingTest),
                CustomViewType = $"marketing-testing/views/{targetView}"
            };

            return true;
        }

        private MarketingTestingContextModel GenerateContextData(IMarketingTest testData)
        {
            var marketingTestingContextModel = new MarketingTestingContextModel();
            
            //set contextmodel IMarketingTest data
            marketingTestingContextModel.Test = testData;

            //convert test data StartDate to local time;
            marketingTestingContextModel.Test.StartDate = marketingTestingContextModel.Test.StartDate.ToLocalTime();
            
            //get published version
            var publishedContentPageData = _contentRepository.Get<PageData>(testData.OriginalItemId);
            var publishedVersionData = _contentVersionRepository.LoadPublished(publishedContentPageData.ContentLink,publishedContentPageData.LanguageBranch);
            
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
                marketingTestingContextModel.DaysElapsed = "";
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
                
                var urlHelper = ServiceLocator.Current.GetInstance<UrlHelper>();
                marketingTestingContextModel.ConversionLink = urlHelper.ContentUrl(conversionContent.ContentLink);
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
    
    [ExcludeFromCodeCoverage]
    internal class MarketingTestContext : ClientContextBase
    {
        public override string DataType { get; set; }
        public string CustomViewType { get; set; }
    }
}
