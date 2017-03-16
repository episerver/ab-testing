using System;
using EPiServer.Marketing.Testing.Web.Models;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.KPI.Manager.DataClass;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    public interface IMarketingTestingWebRepository
    {
        IMarketingTest GetTestById(Guid testGuid);
        List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId);
        List<IMarketingTest> GetTestList(TestCriteria criteria);
        Guid CreateMarketingTest(TestingStoreModel testData);
        void DeleteMarketingTest(Guid testGuid);
        void StartMarketingTest(Guid testGuid);
        void StopMarketingTest(Guid testGuid);
        void ArchiveMarketingTest(Guid testObjectId, Guid winningVariantId);
        Guid SaveMarketingTest(IMarketingTest testData);
        IMarketingTest GetActiveTestForContent(Guid contentGuid);
        void DeleteTestForContent(Guid contentGuid);
        string PublishWinningVariant(TestResultStoreModel testResult);
        Variant ReturnLandingPage(Guid testId);
        IContent GetVariantContent(Guid contentGuid);
        void AsynchronousIncrementCount(Guid testId, int itemVersion, CountType resultType);
        void IncrementCount(Guid testId, int itemVersion, CountType resultType);
        void AsynchronousSaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type);
        void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type);
        List<IMarketingTest> GetActiveCachedTests();
        IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e);



    }
}
