using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Web.Models;
using StructureMap.TypeRules;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    public class KpiWebRepository : IKpiWebRepository
    {
        /// <summary>
        /// Retrieves all KPI's available to the system.
        /// </summary>
        /// <returns></returns>
        public List<KpiTypeModel> GetKpiTypes()
        {
            IKpiManager kpiManager = ServiceLocator.Current.GetInstance<IKpiManager>();
            List<KpiTypeModel> kpiData = new List<KpiTypeModel>();

            var KpiTypes = kpiManager.GetAllKpis();
            foreach (Type t in KpiTypes)
            {
                kpiData.Add(new KpiTypeModel() {kpi = Activator.CreateInstance(t) as IKpi, kpiType = t});
            }
            return kpiData;
        }

    }
}
