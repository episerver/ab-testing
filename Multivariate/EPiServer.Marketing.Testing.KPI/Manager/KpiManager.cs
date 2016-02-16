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

        public Kpi Get(Guid testObjectId)
        {
            return _dataAccess.Get(testObjectId);
        }

        public List<Kpi> GetKpiByItemId(Guid originalItemId)
        {
            return _dataAccess.GetKpiByItemId(originalItemId);
        }

        public List<Kpi> GetKpiList()
        {
            return _dataAccess.GetKpiList();
        }
        public Guid Save(Kpi multivariateTest)
        {
            return _dataAccess.Save(multivariateTest);
        }

        public void Delete(Guid testObjectId)
        {
            _dataAccess.Delete(testObjectId);
        }

      
    }
}
