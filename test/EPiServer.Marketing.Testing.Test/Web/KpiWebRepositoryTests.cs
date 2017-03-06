using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class KpiWebRepositoryTests
    {
        private Mock<IServiceLocator> _mockServiceLocator;
        private Mock<IKpiManager> _mockKpiManager;
        private List<Type> _kpiTypes;

        private KpiWebRepository GetUnitUnderTest()
        {
            _mockServiceLocator = new Mock<IServiceLocator>();

            _kpiTypes = new List<Type>();//{   Name = "ContentComparatorKPI" FullName = "EPiServer.Marketing.KPI.Common.ContentComparatorKPI" };
            _kpiTypes.Add(typeof(ContentComparatorKPI));
            _kpiTypes.Add(typeof(StickySiteKpi));

            _mockKpiManager = new Mock<IKpiManager>();
            _mockKpiManager.Setup(call => call.GetKpiTypes()).Returns(_kpiTypes);
            _mockServiceLocator.Setup(sl => sl.GetInstance<IKpiManager>()).Returns(_mockKpiManager.Object);

            ServiceLocator.SetLocator(_mockServiceLocator.Object);

            var aRepo = new KpiWebRepository();
            return aRepo;
        }

        [Fact]
        public void GetKpiTypes_Returns_Correct_List_Of_Types()
        {
            var webRepo = GetUnitUnderTest();

            var kpiTypes = webRepo.GetKpiTypes();

            Assert.Equal(2, kpiTypes.Count);
        }
    }
}
