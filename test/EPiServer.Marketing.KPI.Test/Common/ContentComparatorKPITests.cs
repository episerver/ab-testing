using EPiServer.Core;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Results;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Test.Fakes;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Xunit;
using EPiServer.Marketing.KPI.Common.Helpers;

namespace EPiServer.Marketing.KPI.Test.Common
{
    public class ContentComparatorKPITests
    {
        private Guid LandingPageGuid = new Guid("A051E0AC-571A-4490-8909-854BA43B8E1E");

        private Mock<IServiceLocator> _serviceLocator = new Mock<IServiceLocator>();
        private Mock<IContentRepository> _contentRepo;
        private Mock<IContentVersionRepository> _contentVersionRepo;
        private Mock<IContentEvents> _contentEvents;
        private Mock<UrlResolver> _urlResolver;
        private Mock<IKpiHelper> _kpiHelper = new Mock<IKpiHelper>();

        private IContent _content;
        private IContent _content2;
        private IContent _content3;
        private IContent _nullContent;
        //private IContent _nullContentResult;

        private ContentComparatorKPI GetUnitUnderTest()
        {
            _content = new BasicContent {ContentLink = new ContentReference(1, 2)};
            _content2 = new BasicContent {ContentLink = new ContentReference(11, 12)};

            var pageRef2 = new PageReference() { ID = 2, WorkID = 5 };
            var contentData = new PageData(pageRef2);
            ContentVersion ver = null;

            _contentRepo = new Mock<IContentRepository>();
            _contentRepo.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(contentData);
            _contentRepo.Setup(call => call.Get<IContent>(It.Is<ContentReference>(cf => cf == _content.ContentLink))).Returns(_content);
            _contentRepo.Setup(call => call.Get<IContent>(It.Is<ContentReference>(cf => cf == _content2.ContentLink))).Returns(_content2);
            _serviceLocator.Setup(sl => sl.GetInstance<IContentRepository>()).Returns(_contentRepo.Object);

            _contentVersionRepo = new Mock<IContentVersionRepository>();
            _contentVersionRepo.Setup(call => call.LoadPublished(It.Is<ContentReference>(cf => cf != _content))).Returns(new ContentVersion(ContentReference.EmptyReference, "", VersionStatus.Published, DateTime.Now, "", "", 1, "", true, false));
            _serviceLocator.Setup(sl => sl.GetInstance<IContentVersionRepository>()).Returns(_contentVersionRepo.Object);

            _serviceLocator.Setup(sl => sl.GetInstance<LocalizationService>()).Returns(new FakeLocalizationService("whatever"));

            _contentEvents = new Mock<IContentEvents>();
            _serviceLocator.Setup(sl => sl.GetInstance<IContentEvents>()).Returns(_contentEvents.Object);

            _urlResolver = new Mock<UrlResolver>();
            _urlResolver.Setup(call => call.GetUrl(It.IsAny<ContentReference>())).Returns("testUrl");
            _serviceLocator.Setup(s1 => s1.GetInstance<UrlResolver>()).Returns(_urlResolver.Object);

            _kpiHelper.Setup(call => call.GetUrl(It.IsAny<ContentReference>())).Returns("testUrl");
            _serviceLocator.Setup(sl => sl.GetInstance<IKpiHelper>()).Returns(_kpiHelper.Object);
            
            return new ContentComparatorKPI(_serviceLocator.Object, LandingPageGuid);
        }

        private ContentComparatorKPI GetUnitUnderTest2()
        {
            _content3 = new BasicContent { ContentLink = new ContentReference(11, 12) };
            _nullContent = new BasicContent { ContentLink = new ContentReference(111, 112) };

            var pageRef2 = new PageReference() { ID = 2, WorkID = 5 };
            var contentData = new PageData(pageRef2);
            ContentVersion ver = null;
            _serviceLocator = new Mock<IServiceLocator>();

            _contentRepo = new Mock<IContentRepository>();
            _contentRepo.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(contentData);
            _contentRepo.Setup(call => call.Get<IContent>(It.Is<ContentReference>(cf => cf == _content3.ContentLink))).Returns(_content3);
            _contentRepo.Setup(call => call.Get<IContent>(It.Is<ContentReference>(cf => cf == _nullContent.ContentLink))).Returns(_nullContent);
            _serviceLocator.Setup(sl => sl.GetInstance<IContentRepository>()).Returns(_contentRepo.Object);

            _contentVersionRepo = new Mock<IContentVersionRepository>();
            _contentVersionRepo.Setup(call => call.LoadPublished(It.Is<ContentReference>(cf => cf == _nullContent))).Returns(ver);
            _serviceLocator.Setup(sl => sl.GetInstance<IContentVersionRepository>()).Returns(_contentVersionRepo.Object);

            _serviceLocator.Setup(sl => sl.GetInstance<LocalizationService>()).Returns(new FakeLocalizationService("whatever"));

            _contentEvents = new Mock<IContentEvents>();
            _serviceLocator.Setup(sl => sl.GetInstance<IContentEvents>()).Returns(_contentEvents.Object);

            _urlResolver = new Mock<UrlResolver>();
            _urlResolver.Setup(call => call.GetUrl(It.IsAny<ContentReference>())).Returns("testUrl");
            _serviceLocator.Setup(s1 => s1.GetInstance<UrlResolver>()).Returns(_urlResolver.Object);

            _kpiHelper.Setup(call => call.GetUrl(It.IsAny<ContentReference>())).Returns("testUrl");
            _serviceLocator.Setup(sl => sl.GetInstance<IKpiHelper>()).Returns(_kpiHelper.Object);

            return new ContentComparatorKPI(_serviceLocator.Object, LandingPageGuid);
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
            Assert.True( kpi.ContentGuid.Equals(LandingPageGuid), "Evaluate should have returned true");
        }

        [Fact]
        public void VerifyGetUiMarkups()
        {
            var kpi = GetUnitUnderTest();
            
            Assert.NotNull(kpi.UiMarkup);
        }

        [Fact]
        public void VerifyGetDefaultConstructor()
        {
            var kpi = GetUnitUnderTest();
            
            Assert.True(kpi.ContentGuid.Equals(LandingPageGuid), "Evaluate should have returned true");
        }

        [Fact]
        public void Kpi_Validate_Coverage()
        {
            Thread.Sleep(1000);
            var kpi = GetUnitUnderTest();
            var data = new Dictionary<string, string>
            {
                {"ConversionPage", _content.ContentLink.ToString()},
                {"CurrentContent", _content2.ContentLink.ToString()}
            };

            kpi.Validate(data);
        }

        [Fact]
        public void Kpi_Validate_IsContentPublished_Throws_Exception()
        {
            var kpi = GetUnitUnderTest2();
            var data = new Dictionary<string, string>
            {
                {"ConversionPage", _nullContent.ContentLink.ToString()},
                {"CurrentContent", _content3.ContentLink.ToString()}
            };

            Assert.Throws<KpiValidationException>(() => kpi.Validate(data));
        }

        [Fact]
        public void Kpi_Validate_IsCurrentContent_Throws_Exception()
        {
            var kpi = GetUnitUnderTest();
            var data = new Dictionary<string, string>
            {
                {"ConversionPage", _content2.ContentLink.ToString()},
                {"CurrentContent", _content2.ContentLink.ToString()}
            };

            Assert.Throws<KpiValidationException>(() => kpi.Validate(data));
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

        [Fact]
        public void Kpi_Converts_IfContentPathEqualsRequestedPath_AndGuidsAreEqual()
        {
            var contentGuid = LandingPageGuid;
            var content3 = new Mock<IContent>();
            content3.SetupGet(get => get.ContentGuid).Returns(LandingPageGuid);
            var arg = new ContentEventArgs(new ContentReference()) { Content = content3.Object };

            var kpi = GetUnitUnderTest();
            _kpiHelper.Setup(call => call.GetUrl(It.IsAny<ContentReference>())).Returns("/ContentPath/");
            _kpiHelper.Setup(call => call.GetRequestPath()).Returns("/ContentPath/");

            var retVal = kpi.Evaluate(new object(), arg);
            Assert.True(retVal.HasConverted);
        }

        [Fact]
        public void Kpi_DoesNotConvert_IfContentPathEqualsRequestedPath_AndGuidsAreNotEqual()
        {
            var contentGuid = Guid.NewGuid();
            var content3 = new Mock<IContent>();
            content3.SetupGet(get => get.ContentGuid).Returns(contentGuid);
            var arg = new ContentEventArgs(new ContentReference()) { Content = content3.Object };

            var kpi = GetUnitUnderTest();
            _kpiHelper.Setup(call => call.GetUrl(It.IsAny<ContentReference>())).Returns("/ContentPath/");
            _kpiHelper.Setup(call => call.GetRequestPath()).Returns("/ContentPath/");

            var retVal = kpi.Evaluate(new object(), arg);
            Assert.False(retVal.HasConverted);
        }

        [Fact]
        public void Kpi_DoesNotConvert_IfContentPathDoesNotEqualRequestedPath_AndGuidsAreNotEqual()
        {
            var contentGuid = Guid.NewGuid();
            var content3 = new Mock<IContent>();
            content3.SetupGet(get => get.ContentGuid).Returns(contentGuid);
            var arg = new ContentEventArgs(new ContentReference()) { Content = content3.Object };

            var kpi = GetUnitUnderTest();
            _kpiHelper.Setup(call => call.GetUrl(It.IsAny<ContentReference>())).Returns("/ContentPath/");
            _kpiHelper.Setup(call => call.GetRequestPath()).Returns("/DifferentContentPath/");

            var retVal = kpi.Evaluate(new object(), arg);
            Assert.False(retVal.HasConverted);
        }

        [Fact]
        public void Kpi_DoesNotConvert_IfContentDoesNotEqualRequestedContent_AndGuidsAreEqual()
        {
            var contentGuid = LandingPageGuid;
            var content3 = new Mock<IContent>();
            content3.SetupGet(get => get.ContentGuid).Returns(LandingPageGuid);
            var arg = new ContentEventArgs(new ContentReference()) { Content = content3.Object };

            var kpi = GetUnitUnderTest();
            _kpiHelper.Setup(call => call.GetUrl(It.IsAny<ContentReference>())).Returns("/ContentPath/");
            _kpiHelper.Setup(call => call.GetRequestPath()).Returns("/DifferentContentPath/");

            var retVal = kpi.Evaluate(new object(), arg);
            Assert.False(retVal.HasConverted);
        }
    }

    public class TestKpi : Kpi
    {
    }
}