using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.KPI.Model;

namespace EPiServer.Marketing.Testing.KPI.DataAccess
{
    public interface IKpiDataAccess
    {
        Kpi Get(Guid kpiObjectId);

        List<Kpi> GetKpiByItemId(Guid originalItemId);

        List<Kpi> GetKpiList();

        Guid Save(Kpi kpiObject);

        void Delete(Guid kpiObjectId);
    }
}
