using System;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Rest;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Core.DataClass;

namespace EPiServer.Marketing.Testing.Web.Context
{
    [ServiceConfiguration(typeof(IUriContextResolver))]
    public class MarketingTestingContextResolver : IUriContextResolver
    {
        public static readonly string UriPrefix = "epi.marketing.testing";
        private readonly IMarketingTestingWebRepository _marketingTestRepository;
        private readonly ITestingContextHelper _testingContextHelper;
        private const string testId = "testid";
        private const string contentId = "contentid";

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
            var resolvedUri = false;
            Guid id;
            IMarketingTest marketingTest = new ABTest();
            instance = null;
            
            if (VerifyUri(uri) && Guid.TryParse(uri.Segments[1].TrimEnd('/').Split('=')[1], out id))
            {
                //if url is properly formatted with the correct data
                //get back the test based on the guid type given
                var idType = uri.Segments[1].Split('=')[0];

                switch (idType)
                {
                    case testId:
                        marketingTest = _marketingTestRepository.GetTestById(id);
                        break;
                    case contentId:
                        marketingTest = _marketingTestRepository.GetActiveTestForContent(id);
                        break;
                }

                //check that a test was found
                if (marketingTest != null)
                {
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
                    resolvedUri = true;
                }
            }

            return resolvedUri;
        }

        /// <summary>
        /// check url to resolve for actionable segments
        /// Segment 1 = guid and guid type
        /// Segment 2 = View to render
        /// </summary>
        /// <param name="uri">Uri to validate</param>
        /// <returns>a flag to indicate if the Uri passed in is acceptable to continue with</returns>
        private bool VerifyUri(Uri uri)
        {
            List<string> validItems = new List<string>() { testId, contentId };
            return (uri.Segments.Length >= 3 && uri.Segments[1].Contains("=") &&  validItems.Contains(uri.Segments[1].Split('=')[0]));
        }
    }

    [ExcludeFromCodeCoverage]
    internal class MarketingTestContext : ClientContextBase
    {
        public override string DataType { get; set; }
        public string CustomViewType { get; set; }
    }
}
