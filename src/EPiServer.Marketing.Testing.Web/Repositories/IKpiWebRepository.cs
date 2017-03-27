using System.Collections.Generic;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.KPI.Manager.DataClass;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    interface IKpiWebRepository
    {
        List<KpiTypeModel> GetKpiTypes();
        List<Dictionary<string, string>> deserializeJsonFormDataCollection(string jsonFormDataCollection);
        IKpi activateKpiInstance(Dictionary<string, string> kpiFormData);


    }
}
