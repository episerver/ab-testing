using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Core;
using System.Collections;
using EPiServer.Marketing.KPI.Manager.DataClass;

namespace EPiServer.Marketing.Testing
{
    public interface ITestManager
    {
        IMarketingTest Get(Guid testObjectId);

        List<IMarketingTest> GetTestByItemId(Guid originalItemId);

        List<IMarketingTest> GetTestList(TestCriteria criteria);

        Guid Save(IMarketingTest testObject);

        void Delete(Guid testObjectId);

        void Start(Guid testObjectId);

        void Stop(Guid testObjectId);

        void Archive(Guid testObjectId);

        void IncrementCount(Guid testId, Guid testItemId, int itemVersion, CountType resultType);

        void EmitUpdateCount(Guid testId, Guid testItemId, int itemVersion, CountType resultType);

        Guid ReturnLandingPage(Guid testId);

        IList<IKpi> EvaluateKPIs(Guid testId, IContent content);
    }
}
