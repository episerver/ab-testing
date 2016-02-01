using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Model.Enums;

namespace EPiServer.Marketing.Testing
{
    public interface IMultivariateTestManager
    {
        IABTest Get(Guid testObjectId);

        List<IABTest> GetTestByItemId(Guid originalItemId);

        List<IABTest> GetTestList(MultivariateTestCriteria criteria);

        Guid Save(IABTest testObject);

        void Delete(Guid testObjectId);

        void Start(Guid testObjectId);

        void Stop(Guid testObjectId);

        void Archive(Guid testObjectId);

        void IncrementCount(Guid testId, Guid testItemId, CountType resultType);
        void EmitUpdateCount(Guid testId, Guid testItemId, CountType resultType);

        Guid ReturnLandingPage(Guid testId);
    }
}
