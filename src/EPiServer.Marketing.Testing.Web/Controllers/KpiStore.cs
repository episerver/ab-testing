using System.Web.Mvc;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.Shell.Services.Rest;
using System;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Manager;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Net;
using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("KpiStore")]
    public class KpiStore : RestControllerBase
    {
        private LocalizationService _localizationService;
        private ILogger _logger;

        public KpiStore()
        {
            _logger = LogManager.GetLogger();
            _localizationService = ServiceLocator.Current.GetInstance<LocalizationService>();
        }

        /// <summary>
        /// Gets KPI types currently available to the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public RestResult Get()
        {
            IKpiWebRepository kpiRepo = new KpiWebRepository();
            return Rest(kpiRepo.GetKpiTypes());
        }

        /// <summary>
        /// Sends Kpi Type and Form Data to the appropriate IKPI instance for
        /// validation and handling.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult put(string id,string entity)
        {
            ActionResult result;
            var javascriptSerializer = new JavaScriptSerializer();
            Dictionary<string, string> values =
                javascriptSerializer.Deserialize<Dictionary<string, string>>(entity);
            if (!string.IsNullOrEmpty(entity) && values["kpiType"] != "")
            {              
                IKpi kpiInstance = Activator.CreateInstance(Type.GetType(values["kpiType"])) as IKpi;
                
                try
                {
                    kpiInstance.Validate(values);
                    KpiManager kpiManager = new KpiManager();
                    kpiInstance.PreferredCommerceFormat = kpiManager.GetCommerceSettings();
                    var kpiId = kpiManager.Save(kpiInstance);
                    result = Rest(kpiId);
                }
                catch (Exception e)
                {
                    _logger.Error("Error creating Kpi" + e);
                    result = new RestStatusCodeResult((int) HttpStatusCode.InternalServerError, e.Message);
                }
            }
            else
            {
                result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError,_localizationService.GetString("/abtesting/addtestview/error_conversiongoal"));

            }
            return result;
        }
    }
}
