using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using StructureMap.TypeRules;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    public class KpiWebRepository : IKpiWebRepository
    {
        public List<IKpi> GetSystemKpis()
        {
            IKpiManager kpiManager = ServiceLocator.Current.GetInstance<IKpiManager>();
            return kpiManager.GetAllKpis();
        }
    }
}
