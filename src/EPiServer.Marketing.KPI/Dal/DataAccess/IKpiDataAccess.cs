using System;
using System.Collections.Generic;
using System.Data.Common;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.DataAccess
{
    public interface IKpiDataAccess
    {
        IDalKpi Get(Guid kpiObjectId);

        List<IDalKpi> GetKpiList();

        Guid Save(IDalKpi kpiObject);

        IList<Guid> Save(IList<IDalKpi> kpiObjects);

        void Delete(Guid kpiObjectId);

        long GetDatabaseVersion(DbConnection dbConnection, string schema, string contextKey);
    }
}
