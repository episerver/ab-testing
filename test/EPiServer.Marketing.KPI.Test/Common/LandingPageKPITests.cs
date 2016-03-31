using EPiServer.Core;
using EPiServer.Marketing.KPI.Common;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace EPiServer.Marketing.KPI.Test.Common
{
    public class LandingPageKPITests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IContentLoader> _contentLoader;
        private Mock<UrlResolver> _urlResolver;

        private Guid LandingPageGuid = new Guid("A051E0AC-571A-4490-8909-854BA43B8E1E");

        private LandingPageKPI GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _contentLoader = new Mock<IContentLoader>();
            _urlResolver = new Mock<UrlResolver>();
            _serviceLocator.Setup(sl => sl.GetInstance<IContentLoader>()).Returns(_contentLoader.Object);
            _serviceLocator.Setup(sl => sl.GetInstance<UrlResolver>()).Returns(_urlResolver.Object);

            return new LandingPageKPI(LandingPageGuid, _serviceLocator.Object);
        }

        [Fact]
        public void Call_Evaluate_ValidateGetsPagePath()
        {
            var kpi = GetUnitUnderTest();
            kpi.Evaluate("http://www.yahoo.com");

            _contentLoader.Verify(cr => cr.Get<IContent>(It.Is<Guid>(arg => arg.Equals(LandingPageGuid))), 
                Times.Once, 
                "Content repository \"get\" was never called or it was but with wrong argument");
            _urlResolver.Verify(ur => ur.GetVirtualPath(It.IsAny<IContent>()), "Failed to call GetVirtualPath");
//            _serviceLocator.Verify( sl => sl.);
        }
    }
}
