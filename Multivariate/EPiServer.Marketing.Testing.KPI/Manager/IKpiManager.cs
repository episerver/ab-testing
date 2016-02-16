using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.KPI.Model;

namespace EPiServer.Marketing.Testing.KPI.Manager
{
    public interface IKpiManager
    {
        Kpi Get(Guid testObjectId);

        List<Kpi> GetKpiByItemId(Guid originalItemId);

        List<Kpi> GetKpiList();

        Guid Save(Kpi testObject);

        void Delete(Guid testObjectId);
    }
}
