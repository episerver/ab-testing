﻿using System;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Marketing.Testing.Web.Config;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [EPiServer.PlugIn.GuiPlugIn(
        Area = EPiServer.PlugIn.PlugInArea.AdminConfigMenu, 
        Url = "/EPiServer/EPiserver.Marketing.Testing.Web/Config/Index", 
        DisplayName = "AB Testing Configuration")]
    public class ConfigController : Controller
    {
        public ActionResult Index()
        {
            var model = AdminConfigTestSettings.Current;

            return View("~/modules/_protected/Episerver.Marketing.Testing/Admin/Config/ConfigView.cshtml", model);
        }

        [HttpPost]
        [MultiButton(MatchFormKey = "action", MatchFormValue = "Save")]
        public ActionResult Save(string action, AdminConfigTestSettings testSettings)
        {
            if (ModelState.IsValid)
            {
                testSettings.Save();
            }

            return View("~/modules/_protected/Episerver.Marketing.Testing/Admin/Config/ConfigView.cshtml", testSettings);
        }

        [HttpPost]
        [MultiButton(MatchFormKey = "action", MatchFormValue = "Cancel")]
        public ActionResult Cancel()
        {
            var model = AdminConfigTestSettings.Current;
            return View("~/modules/_protected/Episerver.Marketing.Testing/Admin/Config/ConfigView.cshtml", model);
        }
    }

    [InitializableModule]
    public class CustomRouteInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            RouteTable.Routes.RouteExistingFiles = true;
            RouteTable.Routes.MapRoute(
                null,
                "EPiServer/EPiserver.Marketing.Testing.Web/{controller}/{action}",
                new { controller = "Config", action = "Index" },
                new[] { "EPiServer.Marketing.Testing.Web.Controllers" });
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MultiButtonAttribute : ActionNameSelectorAttribute
    {
        public string MatchFormKey { get; set; }
        public string MatchFormValue { get; set; }

        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo)
        {
            return controllerContext.HttpContext.Request[MatchFormKey] != null &&
                controllerContext.HttpContext.Request[MatchFormKey] == MatchFormValue;
        }
    }
}

