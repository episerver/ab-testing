﻿using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.DataAccess;
using Moq;
using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.KPI.Dal.Model;
using Xunit;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Core;

namespace EPiServer.Marketing.KPI.Test
{
    public class KPIManagerTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IKpiDataAccess> _kpiDataAccess;

        private DalKpi GetDalKpi()
        {
            return new DalKpi()
            {
                ClassName = "EPiServer.Marketing.KPI.Manager.DataClass.Kpi, EPiServer.Marketing.KPI",
                Properties = "{ \"Id\":\"fa76a408-1fb4-44a9-9231-954961f0676b\", \"CreatedDate\":\"2016-04-08T18:24:30.3161712Z\", \"ModifiedDate\":\"2016-04-08T18:24:30.3161712Z\" }"
            };
        }

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
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            _kpiDataAccess.Setup(dal => dal.Get(It.IsAny<Guid>())).Returns(GetDalKpi());
            tm.Get(theGuid);

            _kpiDataAccess.Verify(da => da.Get(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer get was never called or Guid did not match.");
        }

        [Fact]
        public void KpiTestManager_CallsSaveWithKpi()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            var kpi = new Kpi()
            {
                Id = theGuid
            };
            tm.Save(kpi);

            _kpiDataAccess.Verify(da => da.Save(It.Is<DalKpi>(arg => arg.Id == theGuid)),
                "DataAcessLayer Save was never called or object did not match.");
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
            var content = new Mock<IContent>();
            ContentEventArgs arg = new ContentEventArgs(new ContentReference()) { Content = content.Object };
            Assert.Throws<NotImplementedException>(() => kpi.Evaluate(this,arg));
        }

    }
}