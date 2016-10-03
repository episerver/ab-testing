using System.Web.Mvc;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.Shell.Services.Rest;
using System;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Manager;

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
            IKpi kpiInstance = Activator.CreateInstance(Type.GetType(entity.KpiType)) as IKpi;
            KpiValidationResult result = kpiInstance.Validate(entity.KpiData);

            if (!result.IsValid)
            {
                return Rest("can't put");
            }

            KpiManager kpiManager = new KpiManager();
            return Rest(kpiManager.Save(kpiInstance));
            
        }
    }
}
