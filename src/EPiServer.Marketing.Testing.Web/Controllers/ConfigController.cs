using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Marketing.Testing.Web.Config;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [EPiServer.PlugIn.GuiPlugIn(Area = EPiServer.PlugIn.PlugInArea.AdminConfigMenu, Url = "/EPiServer/EPiserver.Marketing.Testing.Web/Config/Index", DisplayName = "AB Testing Configuration")]
    public class ConfigController : Controller
    {
        public ActionResult Index()
        {
            var model = new AdminConfigTestSettings() { TestDuration = 30, ConfidenceLevel = 98, ParticipationPercentage = 15 };

            return View("~/modules/_protected/Episerver.Marketing.Testing/ClientResources/views/ConfigView.cshtml", model);
        }
    }

    [InitializableModule]
    public class CustomRouteInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            RouteTable.Routes.MapRoute(
                null,
                "EPiServer/EPiserver.Marketing.Testing.Web/Config/Index",
                new { controller = "Config", action = "Index" },
                new[] { "EPiServer.Marketing.Testing.Web.Controllers" });
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}

