﻿using System.Web.Mvc;
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
using EPiServer.Marketing.KPI.Exceptions;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("KpiStore")]
    public class KpiStore : RestControllerBase
    {
        private LocalizationService _localizationService;
        private IKpiWebRepository _kpiRepo;
        private IServiceLocator _serviceLocator;
        private ILogger _logger;

        public KpiStore()
        {
            _serviceLocator = ServiceLocator.Current;
            _logger = _serviceLocator.GetInstance<ILogger>();
            _localizationService = _serviceLocator.GetInstance<LocalizationService>();
            _kpiRepo = _serviceLocator.GetInstance<IKpiWebRepository>();
        }

        internal KpiStore(IServiceLocator sl)
        {
            _serviceLocator = sl;
            _logger = _serviceLocator.GetInstance<ILogger>();
            _localizationService = _serviceLocator.GetInstance<LocalizationService>();
            _kpiRepo = sl.GetInstance<IKpiWebRepository>();
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
            List<IKpi> validKpiInstances = new List<IKpi>();
            Dictionary<string, string> kpiErrors = new Dictionary<string, string>();
            ActionResult result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError, _localizationService.GetString("/abtesting/addtestview/error_conversiongoal"));

            try
            {
                var kpiData = _kpiRepo.DeserializeJsonKpiFormCollection(entity);

                if (kpiData.Count > 0)
                {
                    foreach (var data in kpiData)
                    {
                        IKpi kpiInstance = _kpiRepo.ActivateKpiInstance(data);
                        try
                        {
                            kpiInstance.Validate(data);
                            validKpiInstances.Add(kpiInstance);
                        }
                        catch (KpiValidationException ex)
                        {
                            _logger.Debug("Error validating Kpi" + ex);
                            kpiErrors.Add(data["widgetID"], ex.Message);
                        }
                    }

                    //Send back only errors or successful results for proper handling
                    if (kpiErrors.Count > 0)
                    {
                        result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError, JsonConvert.SerializeObject(kpiErrors));
                    }
                    else
                    {
                        result = Rest(_kpiRepo.SaveKpis(validKpiInstances));
                    }
                }
                else
                {
                    result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError, _localizationService.GetString("/abtesting/addtestview/error_conversiongoal"));
                }
            }
            catch (Exception ex)
            {
                _logger.Debug("Error creating Kpi" + ex);
                result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError, ex.Message);
            }           
            return result;
        }
    }
}
