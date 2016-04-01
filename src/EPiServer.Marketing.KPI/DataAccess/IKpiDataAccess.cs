using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.DataAccess
{
    public interface IKpiDataAccess
    {
        IDalKpi Get(Guid kpiObjectId);

        List<IDalKpi> GetKpiList();

        Guid Save(IDalKpi kpiObject);

        void Delete(Guid kpiObjectId);
    }
}
