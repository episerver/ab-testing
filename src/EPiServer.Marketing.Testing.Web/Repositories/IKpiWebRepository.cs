using System.Collections.Generic;
using EPiServer.Marketing.Testing.Web.Models;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    interface IKpiWebRepository
    {
        List<KpiTypeModel> GetKpiTypes();
    }
}
