using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using EPiServer.Cms.Shell.UI.Rest;
using EPiServer.Cms.Shell.UI.Rest.ContentQuery;
using EPiServer.Cms.Shell.UI.Rest.Models;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.Testing.Dal.Mappings;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Rest;
using EPiServer.Shell.Web.Mvc.Html;
using EPiServer.Web.Mvc.Html;


namespace EPiServer.Marketing.Testing.Web.Context
{
    [ServiceConfiguration(typeof(IUriContextResolver))]
    public class MarketingTestingContextResolver : IUriContextResolver
    {
        public static readonly string UriPrefix = "epi.marketing.testing";
        private readonly IMarketingTestingWebRepository _marketingTestRepository;
        private readonly IContentRepository _contentRepository;


        [ExcludeFromCodeCoverage]
        public MarketingTestingContextResolver(ITestManager testManager)
        {
            var serviceLocator = ServiceLocator.Current;
            _marketingTestRepository = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            _contentRepository = serviceLocator.GetInstance<IContentRepository>();
        }

        internal MarketingTestingContextResolver(IServiceLocator serviceLocator)
        {
            _marketingTestRepository = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            _contentRepository = serviceLocator.GetInstance<IContentRepository>();

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

            MarketingTestingContextModel contextModel = new MarketingTestingContextModel();

            contextModel.Test = testData;
            var publishedContent = _contentRepository.Get<IContent>(testData.OriginalItemId) as PageData;
            IContent mycontent;
            var publishedContent3 = _contentRepository.TryGet(publishedContent.ContentLink, out mycontent);
          //  var publishedContent2 = _contentRepository.Get<IContent>(testData.OriginalItemId);
            
           // ContentStoreModelCreator modelCreator = ServiceLocator.Current.GetInstance<ContentStoreModelCreator>();
         //   var mymodel = modelCreator.CreateContentDataStoreModel<ContentDataStoreModel>(publishedContent2, new DefaultQueryParameters());
            //var published = mymodel.PublishedBy;

            if (publishedContent != null)
            {
                contextModel.PublishedVersionContentLink = publishedContent.ContentLink.ToString();
                contextModel.PublishedVersionName = publishedContent.Name;
                contextModel.PublishedVersionPublishedBy = "Need to find publishedBy property";
                contextModel.PublishedVersionPublishedDate = publishedContent.StartPublish.ToString(CultureInfo.CurrentCulture);

                var tempContentClone = publishedContent.ContentLink.CreateWritableClone();
                int variantVersion = testData.Variants.First(x => !x.ItemVersion.Equals(publishedContent.ContentLink.ID)).ItemVersion;
                tempContentClone.WorkID = variantVersion;

                var draftContent = _contentRepository.Get<PageData>(tempContentClone);
                
                contextModel.DraftVersionContentLink = draftContent.ContentLink.ToString();
                contextModel.DraftVersionName = draftContent.PageName;
                contextModel.DraftVersionChangedBy = draftContent.ChangedBy;
                contextModel.DraftVersionChangedDate = draftContent.Saved.ToString(CultureInfo.CurrentCulture);
            }

            contextModel.Test.StartDate = contextModel.Test.StartDate.ToLocalTime();

            if (testData.State == TestState.Active)
            {
                contextModel.DaysElapsed = "";
                contextModel.DaysRemaining = Math.Round(DateTime.Parse(contextModel.Test.EndDate.ToString()).Subtract(DateTime.Now).TotalDays).ToString(CultureInfo.CurrentCulture);
            }
            else if (testData.State == TestState.Inactive)
            {
                contextModel.DaysElapsed = "Test has not been started";
                contextModel.DaysRemaining = "Test has not been started";
            }

            contextModel.VisitorPercentage = testData.ParticipationPercentage.ToString();
            foreach (var variant in testData.Variants)
            {
                contextModel.TotalParticipantCount += variant.Views;
            }

            var kpi = testData.KpiInstances[0] as ContentComparatorKPI;
            var conversionContent = _contentRepository.Get<IContent>(kpi.ContentGuid);
            
            
            var urlHelper = ServiceLocator.Current.GetInstance<UrlHelper>();
            contextModel.ConversionLink = urlHelper.ContentUrl(conversionContent.ContentLink);
            contextModel.ConversionContentName = conversionContent.Name;

            return contextModel;
        }

    
    }


    [ExcludeFromCodeCoverage]
    internal class MarketingTestContext : ClientContextBase
    {
        public override string DataType { get; set; }
        public string CustomViewType { get; set; }
    }

    


}
