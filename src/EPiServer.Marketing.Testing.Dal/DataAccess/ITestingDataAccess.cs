using System;
using System.Collections.Generic;
using System.Data.Common;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;

namespace EPiServer.Marketing.Testing.Dal.DataAccess
{
    public interface ITestingDataAccess
    {
        IABTest Get(Guid testObjectId);

        List<IABTest> GetTestByItemId(Guid originalItemId);

        List<IABTest> GetTestList(DalTestCriteria criteria);

        Guid Save(IABTest testObject);

        void Delete(Guid testObjectId);

        IABTest Start(Guid testObjectId);

        void Stop(Guid testObjectId);

        void Archive(Guid testObjectId, Guid winningVariantId);

        void IncrementCount(Guid testId, Guid testItemId, int itemVersion, DalCountType resultType);

        void AddKpiResultData(Guid testId, Guid testItemId, int itemVersion, IDalKeyResult keyResult, int type);

        long GetDatabaseVersion(DbConnection dbConnection, string schema, string contextKey);
    }
}
