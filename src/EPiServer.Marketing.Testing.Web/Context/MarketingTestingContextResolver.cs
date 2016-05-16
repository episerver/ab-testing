using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Rest;

namespace EPiServer.Marketing.Testing.Web.Context
{
    [ServiceConfiguration(typeof(IUriContextResolver))]
    public class MarketingTestingContextResolver : IUriContextResolver
    {
        public static readonly string UriPrefix = "epi.marketing.testing";
        private readonly ITestManager _testManager;

        [ExcludeFromCodeCoverage]
        public MarketingTestingContextResolver(ITestManager testManager)
        {
            _testManager = testManager;
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

            //check url to resolve for actionable keys
            if (uri.Segments.Length <= 1 || (!uri.Segments[1].Contains("testid") &&
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
            if (!Guid.TryParse(uri.Segments[1].Split('=')[1], out id))
            {
                return false;
            }

            //if url is properly formatted with the correct data
            //get back the test based on the guid type given
            var idType = uri.Segments[1].Split('=')[0];
            id = Guid.Parse(uri.Segments[1].Split('=')[1]);

            switch (idType)
            {
                case "testid":
                    marketingTest = _testManager.Get(id);
                    break;
                case "contentid":
                    marketingTest = _testManager.GetTestByItemId(id).FirstOrDefault();
                    break;
            }

            if (marketingTest == null)
            {
                return false;
            }

            instance = new MarketingTestContext
            {
                Uri = uri,
                RequestedUri = uri,
                VersionAgnosticUri = uri,
                Name = marketingTest.Title,
                DataType = typeof(MarketingTestingContextModel).FullName.ToLowerInvariant(),
                Data = GenerateContextData(marketingTest),
                CustomViewType = "marketing-testing/views/MarketingTestDetailsView"
            };

            return true;
        }

        private MarketingTestingContextModel GenerateContextData(IMarketingTest testData)
        {
            var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();

            MarketingTestingContextModel contextModel = new MarketingTestingContextModel();

            contextModel.Test = testData;
            var publishedContent = contentRepository.Get<IContent>(testData.OriginalItemId) as PageData;
            if (publishedContent != null)
            {
                contextModel.PublishedVersionContentLink = publishedContent.ContentLink.ToString();
                contextModel.PublishedVersionName = publishedContent.Name;


                var tempContentClone = publishedContent.ContentLink.CreateWritableClone();
                int variantVersion = testData.Variants.First(x => !x.ItemVersion.Equals(publishedContent.ContentLink.ID)).ItemVersion;
                tempContentClone.WorkID = variantVersion;

                var draftContent = contentRepository.Get<PageData>(tempContentClone);
                contextModel.DraftVersionContentLink = draftContent.ContentLink.ToString();
                contextModel.DraftVersionName = draftContent.PageName;
                contextModel.DraftVersionChangedBy = draftContent.ChangedBy;
                contextModel.DraftVersionChangedDate = draftContent.Saved.ToString();
            }
            
            
            if (testData.State == TestState.Active)
            {
                contextModel.DaysElapsed = DateTime.Now.Subtract(testData.StartDate).TotalDays.ToString();
                contextModel.DaysRemaining = testData.StartDate.Subtract(DateTime.Now).TotalDays.ToString();
            }
            else if (testData.State == TestState.Inactive)
            {
                contextModel.DaysElapsed = "Test has not been started";
                contextModel.DaysRemaining = "Test has not been started";
            }
            else
            {
                contextModel.DaysElapsed = $"Test finished on {testData.EndDate}";
                contextModel.DaysRemaining = $"Test finished on {testData.EndDate}";
            }

            contextModel.VisitorPercentage = testData.ParticipationPercentage.ToString();
            foreach (var variant in testData.Variants)
            {
                contextModel.TotalParticipantCount += variant.Views;
            }

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
