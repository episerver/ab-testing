using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Dal.Model;
using EPiServer.Marketing.KPI.Dal.Model.Enums;
using EPiServer.Marketing.KPI.DataAccess;

namespace EPiServer.Marketing.KPI.Test
{
    public class KpiTestBase
    {
        public void AddObjectsToContext<T>(KpiTestContext context, IList<T> data) where T : class
        {
            context.Set<T>().AddRange(data);
        }

        public IList<DalKpi> AddMultivariateTests(KpiTestContext context, int numberOfTests)
        {
            var kpis = new List<DalKpi>();

            for (var i = 0; i < numberOfTests; i++)
            {
                kpis.Add(new DalKpi()
                {
                    Id = Guid.NewGuid(),
                    Properties = "test"
                    
                });
            };

            AddObjectsToContext(context, kpis);
            context.SaveChanges();
            return kpis;
        }

        public IList<DalKpi> AddMultivariateTests(KpiDataAccess mtmManager, int numberOfTests)
        {
            var kpis = new List<DalKpi>();

            for (var i = 0; i < numberOfTests; i++)
            {
                var test = new DalKpi()
                {
                    Id = Guid.NewGuid(),
                    Properties = "test"
                };

                mtmManager.Save(test);
                kpis.Add(test);
            }

            return kpis;
        }

       
    }
}
