using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IKpiWebRepository), Lifecycle = ServiceInstanceScope.Singleton)]
    public class KpiWebRepository : IKpiWebRepository
    {
        private IServiceLocator _locator;
        public KpiWebRepository()
        {
            _locator = ServiceLocator.Current;
        }

        public KpiWebRepository(IServiceLocator sl)
        {
            _locator = sl;
        }

        /// <summary>
        /// Retrieves all KPI's available to the system.
        /// </summary>
        /// <returns></returns>
        public List<KpiTypeModel> GetKpiTypes()
        {
            IKpiManager kpiManager = _locator.GetInstance<IKpiManager>();
            List<KpiTypeModel> kpiData = new List<KpiTypeModel>();

            var KpiTypes = kpiManager.GetKpiTypes();
            foreach (Type t in KpiTypes)
            {
                kpiData.Add(new KpiTypeModel() {kpi = Activator.CreateInstance(t) as IKpi, kpiType = t});
            }
            return kpiData;
        }
    }
}
