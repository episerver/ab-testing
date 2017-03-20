using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Marketing.Testing.Web.Controllers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class KpiStoreTests
    {
        Mock<IServiceLocator> _locator = new Mock<IServiceLocator>();
        Mock<ILogger> _logger = new Mock<ILogger>();
        private Mock<IKpiWebRepository> _kpiWebRepoMock;
        private Mock<IKpiManager> _kpiManagerMock;

        private KpiStore GetUnitUnderTest()
        {
            _locator.Setup(sl => sl.GetInstance<ILogger>()).Returns(_logger.Object);
            _locator.Setup(sl => sl.GetInstance<LocalizationService>()).Returns(new FakeLocalizationService("testing"));

            _kpiWebRepoMock = new Mock<IKpiWebRepository>();
            _kpiWebRepoMock.Setup(call => call.GetKpiTypes()).Returns(new List<KpiTypeModel>() { new KpiTypeModel()});
            _locator.Setup(s1 => s1.GetInstance<IKpiWebRepository>()).Returns(_kpiWebRepoMock.Object);

            _kpiManagerMock = new Mock<IKpiManager>();
            _kpiManagerMock.Setup(call => call.Save(It.IsAny<List<IKpi>>())).Returns(new List<Guid>() { Guid.NewGuid()});

            ServiceLocator.SetLocator(_locator.Object);

            var testStore = new KpiStore();
            return testStore;
        }

        [Fact]
        public void Get_Returns_Valid_Response()
        {
            var testClass = GetUnitUnderTest();

            var retResult = testClass.Get();

            Assert.NotNull(retResult.Data);
            Assert.IsType<RestResult>(retResult);
        }

        [Fact]
        public void Put_With_Null_Entity()
        {
            var testClass = GetUnitUnderTest();

            var retResult = testClass.Put("", "") as RestStatusCodeResult;

            Assert.Equal((int)HttpStatusCode.InternalServerError, retResult.StatusCode);
        }

        [Fact]
        public void Put_With_Non_Null_Entity_Throws_Exception()
        {
            var testClass = GetUnitUnderTest();

            var entity =
                "{\"kpiType\": \"EPiServer.Marketing.KPI.Common.ContentComparatorKPI, EPiServer.Marketing.KPI, Version=2.0.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7\",\"ConversionPage\": \"16\",\"CurrentContent\": \"6_197\"}";
            var retResult = testClass.Put("", entity) as RestStatusCodeResult;

            Assert.Equal((int)HttpStatusCode.InternalServerError, retResult.StatusCode);
        }
    }
}
