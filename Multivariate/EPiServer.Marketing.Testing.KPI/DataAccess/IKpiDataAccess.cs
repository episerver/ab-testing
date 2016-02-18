using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.KPI.Model;

namespace EPiServer.Marketing.Testing.KPI.DataAccess
{
    public interface IKpiDataAccess
    {
        IKpi Get(Guid kpiObjectId);

        List<IKpi> GetKpiList();

        Guid Save(IKpi kpiObject);

        void Delete(Guid kpiObjectId);
    }
}
