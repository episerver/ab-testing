using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;
using System;
using System.Collections.Generic;

namespace EPiServer.Marketing.Multivariate.Dal
{
    public interface IMultiVariantDataAccess
    {
        IMultivariateTest Get(Guid testObjectId);

        List<IMultivariateTest> GetTestByItemId(Guid originalItemId);

        List<IMultivariateTest> GetTestList(MultivariateTestCriteria criteria);

        Guid Save(IMultivariateTest testObject);

        void Delete(Guid testObjectId);

        void Start(Guid testObjectId);

        void Stop(Guid testObjectId);

        void Archive(Guid testObjectId);

        void IncrementCount(Guid testId, Guid testItemId, CountType resultType);
    }
}
