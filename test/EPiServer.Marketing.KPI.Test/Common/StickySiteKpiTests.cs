using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.SessionState;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Marketing.KPI.Test.Fakes;
using EPiServer.ServiceLocation;
using EPiServer.Shell;
using EPiServer.Web.Routing;
using Moq;
using Xunit;

namespace EPiServer.Marketing.KPI.Test.Common
{
    public class StickySiteKpiTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IContentRepository> _contentRepo;
        private Mock<IContentVersionRepository> _contentVersionRepo;
        private Mock<IContentEvents> _contentEvents;
        private Mock<UrlResolver> _urlResolver;
        internal Mock<KpiHelper> _stickyHelperMock;
        private IContent _content;
        private IContent _content2;
        private IContent _nullContent;
        
        private StickySiteKpi GetUnitUnderTest()
        {
            _content = new BasicContent { ContentLink = new ContentReference(1, 2) };
            _content2 = new BasicContent { ContentLink = new ContentReference(11, 12) };
            _nullContent = new BasicContent { ContentLink = new ContentReference(111, 112) };
            _stickyHelperMock = new Mock<KpiHelper>();
            _stickyHelperMock.Setup(call => call.IsInSystemFolder()).Returns(false);

            var pageRef2 = new PageReference() { ID = 2, WorkID = 5 };
            var contentData = new PageData(pageRef2);
            ContentVersion ver = null;
            _serviceLocator = new Mock<IServiceLocator>();

            _contentRepo = new Mock<IContentRepository>();
            _contentRepo.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(contentData);
            _contentRepo.Setup(call => call.Get<IContent>(It.Is<ContentReference>(cf => cf == _content.ContentLink))).Returns(_content);
            _contentRepo.Setup(call => call.Get<IContent>(It.Is<ContentReference>(cf => cf == _content2.ContentLink))).Returns(_content2);
            _contentRepo.Setup(call => call.Get<IContent>(It.Is<ContentReference>(cf => cf == _nullContent.ContentLink))).Returns(_nullContent);
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

            ServiceLocator.SetLocator(_serviceLocator.Object);

            return new StickySiteKpi(_stickyHelperMock.Object);
        }

        [Fact]
        public void StickySiteKpi_Throws_Exception_For_Empty_String()
        {
            var kpi = GetUnitUnderTest();
            var data = new Dictionary<string, string>
            {
                {"Timeout", ""},
                {"CurrentContent", _content2.ContentLink.ToString()}
            };

            Assert.Throws<KpiValidationException>(() => kpi.Validate(data));
        }

        [Fact]
        public void StickySiteKpi_Throws_Exception_For_Invalid_Timeout()
        {
            var kpi = GetUnitUnderTest();
            var data = new Dictionary<string, string>
            {
                {"Timeout", null},
                {"CurrentContent", _content2.ContentLink.ToString()}
            };

            Assert.Throws<KpiValidationException>(() => kpi.Validate(data));
        }

        [Fact]
        public void StickySiteKpi_Valid_Data()
        {
            var kpi = GetUnitUnderTest();
            var data = new Dictionary<string, string>
            {
                {"Timeout", "5"},
                {"CurrentContent", _content2.ContentLink.ToString()}
            };

            kpi.Validate(data);

            Assert.Equal(5, kpi.Timeout);
        }

        [Fact]
        public void StickySiteKpi_GetUIMarkups()
        {
            var kpi = GetUnitUnderTest();

            Assert.NotNull(kpi.UiMarkup);
        }


        [Fact]
        public void StickySiteKpi_AddSessionOnLoadedContent()
        {
            HttpContext.Current = FakeHttpContext.FakeContext("http://localhost:48594/alloy-plan/");

            var kpi = GetUnitUnderTest();

            kpi.AddSessionOnLoadedContent(new object(), new ContentEventArgs(new BasicContent()));
        }

        [Fact]
        public void StickySiteKpi_Evaluate()
        {
            var t = FakeHttpContext.FakeContext("http://localhost:48594/alloy-plan/");
            t.Request.Cookies.Add(new HttpCookie("SSK_" + Guid.Empty));
            HttpContext.Current = t;

            var kpi = GetUnitUnderTest();

            var content3 = new Mock<IContent>();
            var arg = new ContentEventArgs(new ContentReference()) { Content = content3.Object };

            var retVal = kpi.Evaluate(new object(), arg);

            Assert.False(retVal.HasConverted, "Evaluate should have returned false");
        }
    }


}
