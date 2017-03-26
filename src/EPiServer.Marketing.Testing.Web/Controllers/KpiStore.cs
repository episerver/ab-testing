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
using System.Linq;
using Newtonsoft.Json;

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

        internal KpiStore(IServiceLocator sl)
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
        [HttpPut]
        public ActionResult Put(string id, string entity)
        {



            IKpi kpiInstance;
            List<IKpi> kpiInstances = new List<IKpi>();
            Dictionary<string, string> kpiErrors = new Dictionary<string, string>();
            List<Dictionary<string, string>> kpiFormData = new List<Dictionary<string, string>>();
            var kpiManager = _serviceLocator.GetInstance<IKpiManager>();
            ActionResult result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError, _localizationService.GetString("/abtesting/addtestview/error_conversiongoal"));
            List<string> jsonResults = new List<string>();

            var javascriptSerializer = new JavaScriptSerializer();

            try
            {
                List<string> values = javascriptSerializer.Deserialize<List<string>>(entity);
                values.ForEach(value =>
                {
                    if (value.Contains("kpiType"))
                        kpiFormData.Add(javascriptSerializer.Deserialize<Dictionary<string, string>>(value));
                });

                var x = kpiFormData;

                if (kpiFormData.Count > 0)
                {
                    foreach (var data in kpiFormData)
                    {
                        var kpi = Activator.CreateInstance(Type.GetType(data["kpiType"]));
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
                            kpiInstance.Validate(data);
                            kpiInstances.Add(kpiInstance);
                            // var kpiIds = kpiManager.Save(new List<IKpi>() { kpiInstance });
                            // result = Rest(kpiIds);
                        }
                        catch (Exception e)
                        {
                            _logger.Error("Error creating Kpi" + e);
                            kpiErrors.Add(data["widgetID"], e.Message);
                            //result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError, e.Message);
                        }
                    }

                    if (kpiErrors.Count > 0)
                    {
                        result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(kpiErrors));
                    }
                    else
                    {
                        var kpiIds = kpiManager.Save(kpiInstances);
                        result = Rest(kpiIds);
                    }


                }
                else
                {
                    result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError, _localizationService.GetString("/abtesting/addtestview/error_conversiongoal"));
                }
            }
            catch (Exception ex)
            {
                result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError, ex.Message);
            }

            var errors = kpiErrors;

            return result;
        }
    }
}
