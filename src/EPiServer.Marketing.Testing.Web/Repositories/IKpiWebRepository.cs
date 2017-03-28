using System.Collections.Generic;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.KPI.Manager.DataClass;
using System;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    interface IKpiWebRepository
    {
        List<KpiTypeModel> GetKpiTypes();
        List<Dictionary<string, string>> DeserializeJsonKpiFormCollection(string jsonFormDataCollection);
        IKpi activateKpiInstance(Dictionary<string, string> kpiFormData);
        Guid SaveKpi(IKpi kpiInstance);
        IList<Guid> SaveKpis(IList<IKpi> kpiInstances);
    }
}
