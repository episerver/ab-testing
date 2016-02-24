using EPiServer.Marketing.KPI.Model;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.DataAccess;
using Moq;
using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.KPI.Model.Enums;
using Xunit;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.KPI.Test
{
    public class KPIManagerTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IKpiDataAccess> _kpiDataAccess;

        private KpiManager GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _kpiDataAccess = new Mock<IKpiDataAccess>();

            _serviceLocator.Setup(sl => sl.GetInstance<IKpiDataAccess>()).Returns(_kpiDataAccess.Object);
            return new KpiManager(_serviceLocator.Object);
        }

        [Fact]
        public void Get_Calls_Into_DataAccess()
        {
            var aGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var manager = GetUnitUnderTest();
            manager.Get(aGuid);
            _kpiDataAccess.Verify(kpiDa => kpiDa.Get(It.Is<Guid>(arg => arg.Equals(aGuid))), "Data access was not called with the expected GUID");
        }

        [Fact]
        public void KpiTestManager_CallsSaveWithKpi()
        {
            var kpi = new Kpi();
            var tm = GetUnitUnderTest();
            tm.Save(kpi);

            _kpiDataAccess.Verify(da => da.Save(It.Is<Kpi>(arg => arg.Equals(kpi))),
                "DataAcessLayer Save was never called or kpi did not match.");
        }

        [Fact]
        public void KpiTestManager_CallsDeleteWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var manager = GetUnitUnderTest();
            manager.Delete(theGuid);

            _kpiDataAccess.Verify(da => da.Delete(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer Delete was never called or Guid did not match.");
        }

        [Fact]
        public void Kpi_Success_Throws_Exception()
        {
            var kpi = new Kpi();
            Assert.Throws<NotImplementedException>(() => kpi.Success(new object()));
        }

    }
}
