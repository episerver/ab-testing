using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.DataAccess
{
    public interface IKpiDataAccess
    {
        IKpi Get(Guid kpiObjectId);

        List<IKpi> GetKpiList();

        Guid Save(IKpi kpiObject);

        void Delete(Guid kpiObjectId);
    }
}
