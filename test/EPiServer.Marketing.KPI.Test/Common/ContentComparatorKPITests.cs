using EPiServer.Core;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Results;
using Moq;
using System;
using System.Collections.Generic;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Test.Fakes;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Xunit;

namespace EPiServer.Marketing.KPI.Test.Common
{
    public class ContentComparatorKPITests
    {
        private Guid LandingPageGuid = new Guid("A051E0AC-571A-4490-8909-854BA43B8E1E");

        private Mock<IServiceLocator> _serviceLocator;
        Mock<IContent> _content = new Mock<IContent>();
        private Mock<IContentRepository> _contentRepo;
        private Mock<IContentVersionRepository> _contentVersionRepo;
        private Mock<IContentEvents> _contentEvents;
        private Mock<UrlResolver> _urlResolver;

        IContent content = new BasicContent();
        IContent content2 = new BasicContent();
        

        private ContentComparatorKPI GetUnitUnderTest()
        {
            content.ContentLink = new ContentReference(1, 2);
            content2.ContentLink = new ContentReference(11, 12);

            var pageRef2 = new PageReference() { ID = 2, WorkID = 5 };
            var contentData = new PageData(pageRef2);
           // var refer = new PageData();// { ContentLink = new ContentReference() { ID = 2, WorkID = 43,}, PageLink = new PageReference()};

            _serviceLocator = new Mock<IServiceLocator>();

            _contentRepo = new Mock<IContentRepository>();
            _contentRepo.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(contentData);
            _contentRepo.Setup(call => call.Get<IContent>(It.Is<ContentReference>(cf => cf == content.ContentLink))).Returns(content);
            _contentRepo.Setup(call => call.Get<IContent>(It.Is<ContentReference>(cf => cf == content2.ContentLink))).Returns(content2);
            _serviceLocator.Setup(sl => sl.GetInstance<IContentRepository>()).Returns(_contentRepo.Object);

            _contentVersionRepo = new Mock<IContentVersionRepository>();
            _contentVersionRepo.Setup(call => call.LoadPublished(It.IsAny<ContentReference>())).Returns(new ContentVersion(ContentReference.EmptyReference, "", VersionStatus.Published, DateTime.Now, "", "", 1, "", true, false));
            _serviceLocator.Setup(sl => sl.GetInstance<IContentVersionRepository>()).Returns(_contentVersionRepo.Object);

            _serviceLocator.Setup(sl => sl.GetInstance<LocalizationService>()).Returns(new FakeLocalizationService("whatever"));

            _contentEvents = new Mock<IContentEvents>();
            _serviceLocator.Setup(sl => sl.GetInstance<IContentEvents>()).Returns(_contentEvents.Object);

            _urlResolver = new Mock<UrlResolver>();
            _urlResolver.Setup(call => call.GetUrl(It.IsAny<ContentReference>())).Returns("testUrl");
            _serviceLocator.Setup(s1 => s1.GetInstance<UrlResolver>()).Returns(_urlResolver.Object);
            
            ServiceLocator.SetLocator(_serviceLocator.Object);

            return new ContentComparatorKPI(LandingPageGuid);
        }

/** Removed for now because code is different in mar-455 and tests 
 * will be merged then.
 * 
        [Fact]
        public void Call_Evaluate_ReturnsTrue()
        {
            var kpi = GetUnitUnderTest();
            _content.SetupGet(c => c.ContentGuid).Returns(LandingPageGuid);
            var result = kpi.Evaluate(this, new ContentEventArgs(new ContentReference()) { Content = _content.Object }) as KpiConversionResult;
            Assert.True(result.HasConverted, "Evaluate should have returned true");
            Assert.Equal(result.KpiId, kpi.Id);
        }

        [Fact]
        public void Call_Evaluate_ReturnsFalse()
        {
            var kpi = GetUnitUnderTest();
            _content.SetupGet(c => c.ContentGuid).Returns(Guid.NewGuid());
            var result = kpi.Evaluate(this, new ContentEventArgs(new ContentReference()) { Content = _content.Object }) as KpiConversionResult;
            Assert.False(result.HasConverted, "Evaluate should have returned false");
        }
*/
        [Fact]
        public void VerifyGet()
        {
            var kpi = GetUnitUnderTest();
            Assert.True( kpi.ContentGuid.Equals(LandingPageGuid));
        }

        [Fact]
        public void VerifyGetDefaultConstructor()
        {
            var kpi = new ContentComparatorKPI()
            {
                ContentGuid = LandingPageGuid
            };

            Assert.True(kpi.ContentGuid.Equals(LandingPageGuid));
        }

        [Fact]
        public void test()
        {
            var kpi = GetUnitUnderTest();
            //IContent content = new BasicContent();
            //content.ContentLink = new ContentReference(1, 2);

            //IContent content2 = new BasicContent();
            //content2.ContentLink = new ContentReference(11, 12);

            var data = new Dictionary<string, string>();
            data.Add("ConversionPage", content.ContentLink.ToString());
            data.Add("CurrentContent", content2.ContentLink.ToString());


            kpi.Validate(data);
        }

        [Fact]
        public void ContentComparator_Evaluate_Doesnt_Convert()
        {
            var kpi = GetUnitUnderTest();

            var content = new Mock<IContent>();
            var arg = new ContentEventArgs(new ContentReference()) { Content = content.Object };

            var retVal = kpi.Evaluate(new object(), arg);

            Assert.False(retVal.HasConverted);
        }

        [Fact]
        public void Kpi_Doesnt_Implement_Evaluate()
        {
            var kpi = new TestKpi();

            Assert.Throws<NotImplementedException>(() => kpi.Evaluate(new object(), new EventArgs()));
        }

        [Fact]
        public void Kpi_Doesnt_Implement_Validate()
        {
            var kpi = new TestKpi();

            Assert.Throws<NotImplementedException>(() => kpi.Validate(new Dictionary<string, string>()));
        }
    }

    public class TestKpi : Kpi
    {
        
    }
}
