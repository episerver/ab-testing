using EPiServer.Marketing.Testing.KPI.Manager;
using EPiServer.Marketing.Testing.KPI.DataAccess;
using Moq;
using System;
using Xunit;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.KPI.Test
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
    }
}
