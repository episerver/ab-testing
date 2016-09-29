using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Manager.DataClass;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    interface IKpiWebRepository
    {
        List<IKpi> GetSystemKpis();
    }
}
