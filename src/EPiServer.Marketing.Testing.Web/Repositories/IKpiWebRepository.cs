using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Web.Models;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    interface IKpiWebRepository
    {
        List<KpiViewModel> GetSystemKpis();
    }
}
