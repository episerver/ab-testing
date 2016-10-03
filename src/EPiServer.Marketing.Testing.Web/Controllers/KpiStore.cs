using System.Web.Mvc;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.Shell.Services.Rest;
using System;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Manager;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("KpiStore")]
    public class KpiStore : RestControllerBase
    {
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
        public RestResult put(string id,KpiStoreArgs entity)
        {
            IKpi kpiInstance = Activator.CreateInstance(Type.GetType(entity.KpiType)) as IKpi;
            var javascriptSerializer = new JavaScriptSerializer();
            Dictionary<string, string> values = javascriptSerializer.Deserialize<Dictionary<string, string>>(entity.KpiJsonFormData);
            KpiValidationResult result = kpiInstance.Validate(values);

            if (!result.IsValid)
            {
                return Rest("can't put");
            }

            KpiManager kpiManager = new KpiManager();
            return Rest(kpiManager.Save(kpiInstance));
            
        }
    }
}
