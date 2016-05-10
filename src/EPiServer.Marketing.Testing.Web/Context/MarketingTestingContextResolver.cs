using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
                DataType = typeof(IMarketingTest).FullName.ToLowerInvariant(),
                Data = marketingTest,
                CustomViewType = "marketing-testing/views/MarketingTestDetailsView"
            };

            return true;
        }

        private void GenerateContextData(IMarketingTest testData)
        {
            var contentLoader = ServiceLocator.Current.GetInstance<IContentRepository>();

            MarketingTestingContextModel contextModel = new MarketingTestingContextModel();

            contextModel.Test = testData;

            contextModel.PublishedContent = contentLoader.Get<ContentData>(testData.OriginalItemId);


            if (testData.State == TestState.Active)
            {
                contextModel.DaysElapsed =(int)DateTime.Now.Subtract(testData.StartDate).TotalDays;
                contextModel.DaysRemaining = (int)testData.StartDate.Subtract(DateTime.Now).TotalDays;
            }
            else
            {
                contextModel.DaysElapsed = 0;
                contextModel.DaysRemaining = 0;
            }

            

        }


    }


    [ExcludeFromCodeCoverage]
    internal class MarketingTestContext : ClientContextBase
    {
        public override string DataType { get; set; }
        public string CustomViewType { get; set; }
    }


}
