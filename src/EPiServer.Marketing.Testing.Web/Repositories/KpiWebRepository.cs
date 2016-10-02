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
        public List<KpiViewModel> GetSystemKpis()
        {
            IKpiManager kpiManager = ServiceLocator.Current.GetInstance<IKpiManager>();
            List<KpiViewModel> kpiData = new List<KpiViewModel>();

            var KpiTypes = kpiManager.GetAllKpis();
            foreach (Type t in KpiTypes)
            {
                kpiData.Add(new KpiViewModel() {kpi = Activator.CreateInstance(t) as IKpi, kpiType = t});
            }
            return kpiData;
        }

        public Guid Save(IKpi kpi)
        {
            return new Guid();
        }
    }
}
