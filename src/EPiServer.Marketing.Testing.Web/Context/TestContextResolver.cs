using System;
using System.Linq;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Rest;

namespace EPiServer.Marketing.Testing.Web.Context
{
    [ServiceConfiguration(typeof(IUriContextResolver))]
    public class TestContextResolver : IUriContextResolver
    {
        public static readonly string UriPrefix = "epi.marketing.testing";
        private readonly ITestManager _testManager;

        public TestContextResolver(ITestManager testManager)
        {
            _testManager = testManager;
        }

        public string Name
        {
            get { return UriPrefix; }
        }

        public bool TryResolveUri(Uri uri, out ClientContextBase instance)
        {
            Guid id = new Guid();
            instance = null;

            if (uri.Segments.Length <= 1 || !Guid.TryParse(uri.Segments[1], out id))
            {
                return false;
            }

           var marketingTest =
                _testManager
                    .GetTestByItemId(id)
                    .FirstOrDefault(x => x.State == TestState.Active);

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
                CustomViewType = "marketing-testing/views/TestDetailsView"
            };

            return true;
        }
    }

    internal class MarketingTestContext : ClientContextBase
    {
        public override string DataType { get; set; }
        public string CustomViewType { get; set; }
    }
}
