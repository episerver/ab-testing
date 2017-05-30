using System;
using EPiServer.Marketing.Testing.Web.Models;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.KPI.Manager.DataClass;
using System.Globalization;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    public interface IMarketingTestingWebRepository
    {
        IMarketingTest GetTestById(Guid testGuid);
        List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId);
        List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId, CultureInfo currentCulture);
        List<IMarketingTest> GetTestList(TestCriteria criteria);
        Guid CreateMarketingTest(TestingStoreModel testData);
        void DeleteMarketingTest(Guid testGuid);
        //void DeleteMarketingTest(Guid testGuid, CultureInfo currentCulture);
        void StartMarketingTest(Guid testGuid);
        void StopMarketingTest(Guid testGuid);
        void ArchiveMarketingTest(Guid testObjectId, Guid winningVariantId);
        Guid SaveMarketingTest(IMarketingTest testData);
        IMarketingTest GetActiveTestForContent(Guid contentGuid);
        IMarketingTest GetActiveTestForContent(Guid contentGuid, CultureInfo currentCulture);
        void DeleteTestForContent(Guid contentGuid);
        void DeleteTestForContent(Guid contentGuid, CultureInfo currentCulture);
        string PublishWinningVariant(TestResultStoreModel testResult);
        Variant ReturnLandingPage(Guid testId);
        IContent GetVariantContent(Guid contentGuid);
        void IncrementCount(Guid testId, int itemVersion, CountType resultType, Guid kpiId = default(Guid), bool async = true);
        void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type, bool async = true);
        List<IMarketingTest> GetActiveCachedTests();
        IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e);
    }
}
