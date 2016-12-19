using System;
using System.Collections.Generic;
using System.Data.Common;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;

namespace EPiServer.Marketing.Testing
{
    public interface ITestManager
    {
        IMarketingTest Get(Guid testObjectId);

        List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId);

        List<IMarketingTest> GetTestByItemId(Guid originalItemId);

        List<IMarketingTest> GetTestList(TestCriteria criteria);

        Guid Save(IMarketingTest testObject);

        void Delete(Guid testObjectId);

        void Start(Guid testObjectId);

        void Stop(Guid testObjectId);

        void Archive(Guid testObjectId, Guid winningVariantId);

        void AddKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type);

        void EmitKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type);

        void IncrementCount(Guid testId, int itemVersion, CountType resultType, bool asynch=true);

        Variant ReturnLandingPage(Guid testId);

        IContent GetVariantContent(Guid contentGuid);

        /// <summary>
        /// Given a list of Kpi's and an EventArg object, each KPI will be evaluated and a list of Kpi instances 
        /// that have been evaluated will be returned.
        /// </summary>
        /// <param name="kpis"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e);

        long GetDatabaseVersion(DbConnection dbConnection, string schema, string contextKey, bool populateCache = false);

        List<IMarketingTest> ActiveCachedTests { get; }
    }
}
