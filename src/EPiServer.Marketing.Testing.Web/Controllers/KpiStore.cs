using System.Web.Mvc;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("KpiStore")]
    public class KpiStore : RestControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            IKpiWebRepository kpiRepo = new KpiWebRepository();
            return Rest(kpiRepo.GetSystemKpis());
        }

        [HttpPut]
        public ActionResult put(string id)
        {
            return Rest("Ive been put");
        }
    }
}
