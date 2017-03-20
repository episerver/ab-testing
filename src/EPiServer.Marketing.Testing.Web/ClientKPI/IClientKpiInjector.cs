using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Web.ClientKPI
{
    internal interface IClientKpiInjector
    {
        void ActivateClientKpis(List<IKpi> kpiInstances, TestDataCookie cookieData);
        void AppendClientKpiScript();
    }
}
