using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EPiServer.Marketing.KPI.Dal.Model;
using EPiServer.Marketing.KPI.Dal.Model.Enums;
using EPiServer.Marketing.KPI.DataAccess;
using EPiServer.ServiceLocation;
using IKpi = EPiServer.Marketing.KPI.Manager.DataClass.IKpi;

namespace EPiServer.Marketing.KPI.Manager
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
            return ConvertToManagerKpi(_dataAccess.Get(kpiId));
        }

        public List<IKpi> GetKpiList()
        {
            return _dataAccess.GetKpiList().Select(dalTest => ConvertToManagerKpi(dalTest)).ToList();
        }

        public Guid Save(IKpi kpi)
        {
            return _dataAccess.Save(ConvertToDalTest(kpi));
        }

        public void Delete(Guid kpiId)
        {
            _dataAccess.Delete(kpiId);
        }


        private Dal.Model.IKpi ConvertToDalTest(IKpi kpi)
        {
            var dalKpi = new Kpi()
            {
                Id = kpi.Id,
                Properties = kpi.Properties,
                CreatedDate = kpi.CreatedDate,
                ModifiedDate = kpi.ModifiedDate
            };

            return dalKpi;
        }

        private IKpi ConvertToManagerKpi(Dal.Model.IKpi kpi)
        {
            var managerKpi = new DataClass.Kpi()
            {
                Id = kpi.Id,
                Properties = kpi.Properties,
                CreatedDate = kpi.CreatedDate,
                ModifiedDate = kpi.ModifiedDate
            };

            return managerKpi;
        }
    }
}
