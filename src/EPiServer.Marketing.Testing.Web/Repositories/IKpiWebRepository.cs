using System.Collections.Generic;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.KPI.Manager.DataClass;
using System;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    public interface IKpiWebRepository
    {
        List<KpiTypeModel> GetKpiTypes();
        List<Dictionary<string, string>> DeserializeJsonKpiFormCollection(string jsonFormDataCollection);
        IKpi ActivateKpiInstance(Dictionary<string, string> kpiFormData);
        Guid SaveKpi(IKpi kpiInstance);
        IList<Guid> SaveKpis(IList<IKpi> kpiInstances);
        IKpi GetKpiInstance(Guid kpiId);
    }
}
