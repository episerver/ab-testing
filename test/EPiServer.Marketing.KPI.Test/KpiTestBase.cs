using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Dal.Model;
using EPiServer.Marketing.KPI.Dal.Model.Enums;
using EPiServer.Marketing.KPI.DataAccess;

namespace EPiServer.Marketing.KPI.Test
{
    public class KpiTestBase
    {
        internal void AddObjectsToContext<T>(KpiTestContext context, IList<T> data) where T : class
        {
            context.Set<T>().AddRange(data);
        }

        internal IList<DalKpi> AddKpis(KpiTestContext context, int numberOfTests)
        {
            var kpis = new List<DalKpi>();

            for (var i = 0; i < numberOfTests; i++)
            {
                kpis.Add(new DalKpi()
                {
                    Id = Guid.NewGuid(),
                    ClassName = "EPiServer.Marketing.KPI.Manager.DataClass.Kpi, EPiServer.Marketing.KPI",
                    Properties = "{ \"Id\":\"fa76a408-1fb4-44a9-9231-954961f0676b\", \"CreatedDate\":\"2016-04-08T18:24:30.3161712Z\", \"ModifiedDate\":\"2016-04-08T18:24:30.3161712Z\" }"

                });
            };

            AddObjectsToContext(context, kpis);
            context.SaveChanges();
            return kpis;
        }

        internal IList<IDalKpi> AddKpis(KpiDataAccess dataAccess, int numberOfTests)
        {
            var kpis = new List<IDalKpi>();

            for (var i = 0; i < numberOfTests; i++)
            {
                var kpi = new DalKpi()
                {
                    Id = Guid.NewGuid(),
                    ClassName = "EPiServer.Marketing.KPI.Manager.DataClass.Kpi, EPiServer.Marketing.KPI",
                    Properties = "{ \"Id\":\"fa76a408-1fb4-44a9-9231-954961f0676b\", \"CreatedDate\":\"2016-04-08T18:24:30.3161712Z\", \"ModifiedDate\":\"2016-04-08T18:24:30.3161712Z\" }"
                };

                
                kpis.Add(kpi);
            }

            dataAccess.Save(kpis);

            return kpis;
        }

       
    }
}
