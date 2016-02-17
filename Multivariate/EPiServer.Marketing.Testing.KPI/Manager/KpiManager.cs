using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.KPI.Model;
using EPiServer.Marketing.Testing.KPI.DataAccess;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.KPI.Manager
{
    [ServiceConfiguration(ServiceType = typeof(IKpiManager), Lifecycle = ServiceInstanceScope.Singleton)]
    public class KpiManager : IKpiManager
    {
        private IKpiDataAccess _dataAccess;
        private IServiceLocator _serviceLocator;

        [ExcludeFromCodeCoverage]
        public KpiManager()
        {
            _serviceLocator = ServiceLocator.Current;
            _dataAccess = new KpiDataAccess();
        }
        internal KpiManager(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _dataAccess = _serviceLocator.GetInstance<IKpiDataAccess>();
        }

        public IKpi Get(Guid kpiId)
        {
            return _dataAccess.Get(kpiId);
        }

        public List<IKpi> GetKpiList()
        {
            return _dataAccess.GetKpiList();
        }
        public Guid Save(IKpi kpi)
        {
            return _dataAccess.Save(kpi);
        }

        public void Delete(Guid kpiId)
        {
            _dataAccess.Delete(kpiId);
        }

      
    }
}
