using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;

namespace EPiServer.Marketing.Testing.Dal
{
    public interface ITestingDataAccess
    {
        IABTest Get(Guid testObjectId);

        List<IABTest> GetTestByItemId(Guid originalItemId);

        List<IABTest> GetTestList(TestCriteria criteria);

        Guid Save(IABTest testObject);

        void Delete(Guid testObjectId);

        void Start(Guid testObjectId);

        void Stop(Guid testObjectId);

        void Archive(Guid testObjectId);

        void IncrementCount(Guid testId, Guid testItemId, int itemVersion, CountType resultType);
    }
}
