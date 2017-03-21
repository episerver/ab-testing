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
        private IServiceLocator _serviceLocator;
        private ILogger _logger;

        public KpiStore()
        {
            _serviceLocator = ServiceLocator.Current;
            _logger = _serviceLocator.GetInstance<ILogger>();
            _localizationService = _serviceLocator.GetInstance<LocalizationService>();
        }

        internal KpiStore( IServiceLocator sl )
        {
            _serviceLocator = sl;
            _logger = _serviceLocator.GetInstance<ILogger>();
            _localizationService = _serviceLocator.GetInstance<LocalizationService>();
        }

        /// <summary>
        /// Gets KPI types currently available to the system
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public RestResult Get()
        {
            var kpiRepo = _serviceLocator.GetInstance<IKpiWebRepository>();
            return Rest(kpiRepo.GetKpiTypes());
        }

        /// <summary>
        /// Sends Kpi Type and Form Data to the appropriate IKPI instance for
        /// validation and handling.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public ActionResult Put(string id, string entity)
        {
            IKpi kpiInstance;
            var kpiManager = _serviceLocator.GetInstance<IKpiManager>();
            ActionResult result;
            var javascriptSerializer = new JavaScriptSerializer();
            Dictionary<string, string> values =
                javascriptSerializer.Deserialize<Dictionary<string, string>>(entity);
            if (!string.IsNullOrEmpty(entity) && values["kpiType"] != "")
            {
                var kpi = Activator.CreateInstance(Type.GetType(values["kpiType"]));

                if (kpi is IFinancialKpi)
                {
                    var financialKpi = kpi as IFinancialKpi;
                    financialKpi.PreferredFinancialFormat = kpiManager.GetCommerceSettings();
                    kpiInstance = financialKpi as IKpi;
                }
                else
                {
                    kpiInstance = kpi as IKpi;
                }

                try
                {
                    kpiInstance.Validate(values);
                    var kpiIds = kpiManager.Save(new List<IKpi>() {kpiInstance});
                    result = Rest(kpiIds);
                }
                catch (Exception e)
                {
                    _logger.Error("Error creating Kpi" + e);
                    result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError, e.Message);
                }
            }else
            {
                result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError, _localizationService.GetString("/abtesting/addtestview/error_conversiongoal"));
            }
            return result;
        }
    }
}
