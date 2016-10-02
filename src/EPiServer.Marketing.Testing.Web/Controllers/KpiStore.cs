using System.Web.Mvc;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("KpiStore")]
    public class KpiStore : RestControllerBase
    {
        
        public RestResult Get()
        {
            IKpiWebRepository kpiRepo = new KpiWebRepository();
            return Rest(kpiRepo.GetSystemKpis());
        }

        public RestResult put(string id,KpiStoreArgs entity)
        {
               
            return Rest("Ive been put");
        }
    }
}
