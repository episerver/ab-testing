using System;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Rest;


namespace EPiServer.Marketing.Testing.Web.Context
{
    [ServiceConfiguration(typeof(IUriContextResolver))]
    public class MarketingTestingContextResolver : IUriContextResolver
    {
        public static readonly string UriPrefix = "epi.marketing.testing";
        private readonly IMarketingTestingWebRepository _marketingTestRepository;
        private readonly ITestingContextHelper _testingContextHelper;


        [ExcludeFromCodeCoverage]
        public MarketingTestingContextResolver()
        {
            var serviceLocator = ServiceLocator.Current;
            _marketingTestRepository = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            _testingContextHelper = new TestingContextHelper();
        }

        internal MarketingTestingContextResolver(IServiceLocator mockServiceLocatorserviceLocator, ITestingContextHelper mockTestingContextHelper)
        {
            _marketingTestRepository = mockServiceLocatorserviceLocator.GetInstance<IMarketingTestingWebRepository>();
            _testingContextHelper = mockTestingContextHelper;

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
                Data = _testingContextHelper.GenerateContextData(marketingTest),
                CustomViewType = $"marketing-testing/views/{targetView}"
            };

            return true;
        }


    }

    [ExcludeFromCodeCoverage]
    internal class MarketingTestContext : ClientContextBase
    {
        public override string DataType { get; set; }
        public string CustomViewType { get; set; }
    }
}
