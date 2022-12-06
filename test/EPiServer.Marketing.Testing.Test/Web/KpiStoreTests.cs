﻿using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Marketing.Testing.Web.Controllers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class KpiStoreTests
    {
        Mock<IServiceLocator> _locator = new Mock<IServiceLocator>();
        Mock<ILogger> _logger = new Mock<ILogger>();
        private Mock<IKpiWebRepository> _kpiWebRepoMock;

        private KpiStore GetUnitUnderTest()
        {
            _locator.Setup(sl => sl.GetInstance<ILogger>()).Returns(_logger.Object);
            _locator.Setup(sl => sl.GetInstance<LocalizationService>()).Returns(new FakeLocalizationService("testing"));

            _kpiWebRepoMock = new Mock<IKpiWebRepository>();
            _kpiWebRepoMock.Setup(call => call.GetKpiTypes()).Returns(new List<KpiTypeModel>() { new KpiTypeModel() });

            _locator.Setup(s1 => s1.GetInstance<IKpiWebRepository>()).Returns(_kpiWebRepoMock.Object);

            var testStore = new KpiStore(_locator.Object);
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

            var retResult = testClass.Put("", "") as RestResult;

            var responseDataStatus = (bool)retResult.Data.GetType().GetProperty("status").GetValue(retResult.Data, null);

            Assert.False(responseDataStatus);
        }

        [Fact]
        public void Put_With_Non_Null_Entity_Throws_Exception()
        {
            var testClass = GetUnitUnderTest();

            var entity =
                "{\"kpiType\": \"EPiServer.Marketing.KPI.Common.ContentComparatorKPI, EPiServer.Marketing.KPI, Version=2.0.0.0, Culture=neutral\",\"ConversionPage\": \"16\",\"CurrentContent\": \"6_197\"}";
            var retResult = testClass.Put("", entity) as RestResult;

            var responseDataStatus = (bool)retResult.Data.GetType().GetProperty("status").GetValue(retResult.Data, null);

            Assert.False(responseDataStatus);
        }

        [Fact]
        public void Put_Returns_Correct_Weight_For_Each_Kpi()
        {
            var testClass = GetUnitUnderTest();

            var dict = new Dictionary<string, string>();
            dict.Add("Timeout", "10");
            dict.Add("kpiType", "EPiServer.Marketing.KPI.Common.StickySiteKpi, EPiServer.Marketing.KPI, Version=2.2.0.0, Culture=neutral");
            dict.Add("widgetID", "KpiWidget_0");
            dict.Add("Weight", "Low");  // equates to weight of 1
            dict.Add("CurrentContent", "9_198");

            var dict2 = new Dictionary<string, string>();
            dict2.Add("TargetDuration", "5");
            dict2.Add("kpiType", "EPiServer.Marketing.KPI.Common.TimeOnPageClientKpi, EPiServer.Marketing.KPI, Version=2.2.0.0, Culture=neutral");
            dict2.Add("widgetID", "KpiWidget_1");
            dict2.Add("Weight", "Medium");  // equates to weight of 2
            dict2.Add("CurrentContent", "9_198");

            var kpis = new List<Dictionary<string, string>>();
            kpis.Add(dict);
            kpis.Add(dict2);

            _kpiWebRepoMock.Setup(call => call.DeserializeJsonKpiFormCollection(It.IsAny<string>())).Returns(kpis);
            var sticky = new Mock<StickySiteKpi>();
            _kpiWebRepoMock.Setup(call => call.ActivateKpiInstance(It.IsAny<Dictionary<string, string>>())).Returns(sticky.Object);

            var retResult = testClass.Put("KpiFormData", "") as RestResult;

            var responseDataObj = (Dictionary<Guid, string>)retResult.Data.GetType().GetProperty("obj").GetValue(retResult.Data, null);

            Assert.Equal(1, responseDataObj.Count(pair => pair.Value == "Low"));
            Assert.Equal(1, responseDataObj.Count(pair => pair.Value == "Medium"));
        }

        [Fact]
        public void Put_With_Duplicate_Kpis_and_Values_Returns_Proper_Errors()
        {
            var testClass = GetUnitUnderTest();

            var dict = new Dictionary<string, string>();
            dict.Add("Timeout", "10");
            dict.Add("kpiType", "EPiServer.Marketing.KPI.Common.StickySiteKpi, EPiServer.Marketing.KPI, Version=2.2.0.0, Culture=neutral");
            dict.Add("widgetID", "KpiWidget_0");
            dict.Add("Weight", "Low");  // equates to weight of 1
            dict.Add("CurrentContent", "9_198");

            var dict2 = new Dictionary<string, string>();
            dict2.Add("Timeout", "10");
            dict2.Add("kpiType", "EPiServer.Marketing.KPI.Common.StickySiteKpi, EPiServer.Marketing.KPI, Version=2.2.0.0, Culture=neutral");
            dict2.Add("widgetID", "KpiWidget_1");
            dict2.Add("Weight", "Medium");  // equates to weight of 2
            dict2.Add("CurrentContent", "9_198");

            var kpis = new List<Dictionary<string, string>>();
            kpis.Add(dict);
            kpis.Add(dict2);

            _kpiWebRepoMock.Setup(call => call.DeserializeJsonKpiFormCollection(It.IsAny<string>())).Returns(kpis);
            var sticky = new Mock<StickySiteKpi>();
            _kpiWebRepoMock.Setup(call => call.ActivateKpiInstance(It.IsAny<Dictionary<string, string>>())).Returns(sticky.Object);

            var retResult = testClass.Put("KpiFormData", "") as RestResult;

            var responseDataErrors = JsonConvert.DeserializeObject<Dictionary<string, string>>(retResult.Data.GetType().GetProperty("errors").GetValue(retResult.Data, null).ToString());

            Assert.Equal(2, responseDataErrors.Count);
            Assert.True(responseDataErrors.Keys.Contains("KpiWidget_0"));
            Assert.True(responseDataErrors["KpiWidget_0"] == "testing");
            Assert.True(responseDataErrors.Keys.Contains("KpiWidget_1"));
            Assert.True(responseDataErrors["KpiWidget_1"] == "testing");
        }
    }
}
