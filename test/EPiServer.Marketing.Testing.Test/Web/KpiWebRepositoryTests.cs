using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Commerce.Kpis;
using EPiServer.Marketing.KPI.Common.Helpers;
using EPiServer.Core;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class KpiWebRepositoryTests
    {
        private Mock<IServiceLocator> _mockServiceLocator;
        private Mock<IKpiManager> _mockKpiManager;
        private List<Type> _kpiTypes;
        private Mock<IKpiHelper> _mockKpiHelper = new Mock<IKpiHelper>();

        private KpiWebRepository GetUnitUnderTest()
        {
            _mockServiceLocator = new Mock<IServiceLocator>();

            _kpiTypes = new List<Type>();//{   Name = "ContentComparatorKPI" FullName = "EPiServer.Marketing.KPI.Common.ContentComparatorKPI" };
            _kpiTypes.Add(typeof(ContentComparatorKPI));
            _kpiTypes.Add(typeof(StickySiteKpi));

            _mockKpiManager = new Mock<IKpiManager>();
            _mockKpiManager.Setup(call => call.GetKpiTypes()).Returns(_kpiTypes);
            _mockServiceLocator.Setup(sl => sl.GetInstance<IKpiManager>()).Returns(_mockKpiManager.Object);

            _mockKpiHelper.Setup(call => call.GetUrl(It.IsAny<ContentReference>())).Returns("testUrl");
            _mockServiceLocator.Setup(sl => sl.GetInstance<IKpiHelper>()).Returns(_mockKpiHelper.Object);


            var aRepo = new KpiWebRepository(_mockServiceLocator.Object);
            return aRepo;
        }

        //[Fact] (Mocking not playing well with Activator.CreateInstance)
        //public void GetKpiTypes_Returns_Correct_List_Of_Types()
        //{
        //    var webRepo = GetUnitUnderTest();

        //    var kpiTypes = webRepo.GetKpiTypes();

        //    Assert.Equal(2, kpiTypes.Count);
        //}

        [Fact]
        public void SaveKpi_Calls_KpiManager_Save_WithSingleKpi()
        {
            var webRepoo = GetUnitUnderTest();
            IKpi testKpi = new Kpi();
            webRepoo.SaveKpi(testKpi);
            _mockKpiManager.Verify(called => called.Save(It.Is<IKpi>(value=>value == testKpi)), Times.Once);
        }

        [Fact]
        public void SaveKpis_Calls_KpiManager_Save_WithList()
        {
            var webRepo = GetUnitUnderTest();
            IKpi testKpi1 = new Kpi();
            IKpi testKpi2 = new Kpi();
            List<IKpi> kpiList = new List<IKpi>() { testKpi1, testKpi2 };
            webRepo.SaveKpis(kpiList);
            _mockKpiManager.Verify(called => called.Save(It.Is<IList<IKpi>>(value => value == kpiList)), Times.Once);
        }

        [Fact]
        public void DeserializeJsonFormDataCollection_returns_empty_list_when_kpiType_empty()
        {
            var jsonString = "[]";
            var webRepo = GetUnitUnderTest();
            var result = webRepo.DeserializeJsonKpiFormCollection(jsonString);
            Assert.True(result.Count == 0);
        }

        [Fact]
        public void DeserializeJsonFormDataCollection_returns_correct_resultList_when_kpiTypes_are_provided()
        {
            var jsonString = "[\n\t\"{\\\"ConversionProduct\\\": \\\"419__CatalogContent\\\",\\\"kpiType\\\": \\\"EPiServer.Marketing.KPI.Commerce.Kpis.AddToCartKpi, EPiServer.Marketing.KPI.Commerce, Version=2.0.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7\\\",\\\"widgetID\\\": \\\"KpiWidget_5\\\",\\\"CurrentContent\\\": \\\"6_304\\\"}\","+
                "\n\t\"{\\\"ConversionProduct\\\": \\\"49__CatalogContent\\\",\\\"kpiType\\\": \\\"EPiServer.Marketing.KPI.Commerce.Kpis.PurchaseItemKpi, EPiServer.Marketing.KPI.Commerce, Version=2.0.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7\\\",\\\"widgetID\\\": \\\"KpiWidget_6\\\",\\\"CurrentContent\\\": \\\"6_304\\\"}\"\n]";
            var webRepo = GetUnitUnderTest();
            var result = webRepo.DeserializeJsonKpiFormCollection(jsonString);
            Assert.True(result.Count == 2);
            Assert.True(result[0]["kpiType"] == "EPiServer.Marketing.KPI.Commerce.Kpis.AddToCartKpi, EPiServer.Marketing.KPI.Commerce, Version=2.0.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7");
            Assert.True(result[1]["kpiType"] == "EPiServer.Marketing.KPI.Commerce.Kpis.PurchaseItemKpi, EPiServer.Marketing.KPI.Commerce, Version=2.0.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7");          
        }

        [Fact]
        public void ActivateKpiInstance_returns_commerce_kpi_when_kpiType_is_commerce()
        {
            Dictionary<string, string> kpiInstanceData = new Dictionary<string, string>();
            kpiInstanceData.Add("kpiType", "EPiServer.Marketing.KPI.Commerce.Kpis.PurchaseItemKpi, EPiServer.Marketing.KPI.Commerce, Version=2.0.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7");
            var webRepo = GetUnitUnderTest();
            var result = webRepo.ActivateKpiInstance(kpiInstanceData);
            Assert.True(result is CommerceKpi);
        }

        [Fact]
        public void ActivateKpiInstance_returns_financial_kpi_when_kpiType_is_financial()
        {
            Dictionary<string, string> kpiInstanceData = new Dictionary<string, string>();
            kpiInstanceData.Add("kpiType", "EPiServer.Marketing.KPI.Commerce.Kpis.AverageOrderKpi, EPiServer.Marketing.KPI.Commerce, Version=2.0.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7");
            var webRepo = GetUnitUnderTest();
            var result = webRepo.ActivateKpiInstance(kpiInstanceData);
            Assert.True(result is IFinancialKpi);
        }

        //[Fact]  (Mocking not playing well with Activator.CreateInstance)
        //public void ActivateKpiInstance_returns_kpi_when_kpiType_is_regularKpi()
        //{
        //    Dictionary<string, string> kpiInstanceData = new Dictionary<string, string>();
        //    kpiInstanceData.Add("kpiType", "EPiServer.Marketing.KPI.Common.ContentComparatorKPI, EPiServer.Marketing.KPI, Version=2.2.0.0, Culture=neutral, PublicKeyToken=8fe83dea738b45b7");
        //    var webRepo = GetUnitUnderTest();
        //    var result = webRepo.ActivateKpiInstance(kpiInstanceData);
        //    Assert.True(result is IKpi);
        //}

        [Fact]
        public void GetKpiInstance_calls_KpiManager_Get()
        {
            Guid testGuid = Guid.Parse("48fe0e90-67f9-4171-a65c-aa00efc0ce77");
            var webRepo = GetUnitUnderTest();
            webRepo.GetKpiInstance(testGuid);
            _mockKpiManager.Verify(call => call.Get(It.Is<Guid>(val => val == testGuid)),Times.Once);
        }
    }
}
