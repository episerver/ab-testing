using System;
using System.Collections.Generic;
using System.Data.Common;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    public interface ITestManager
    {
        /// <summary>
        /// Gets a test based on the supplied id from the database.
        /// </summary>
        /// <param name="testObjectId">Id of the test to retrieve.</param>
        /// <returns>IMarketing Test</returns>
        IMarketingTest Get(Guid testObjectId);

        /// <summary>
        /// Retrieves all active tests that have the supplied OriginalItemId from the cache.  The associated data for each 
        /// test returned may not be current.  If the most current data is required 'Get' should be used instead.
        /// </summary>
        /// <param name="originalItemId">Id of the item under test.</param>
        /// <returns>List of tests</returns>
        List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId);

        /// <summary>
        /// Retrieves all tests, regardless of test state, that have the supplied OriginalItemId.
        /// </summary>
        /// <param name="originalItemId">Id of the item under test.</param>
        /// <returns>List of tests.</returns>
        List<IMarketingTest> GetTestByItemId(Guid originalItemId);

        /// <summary>
        /// Don't want to use refernce the cache here.  The criteria could be anything, not just active tests which
        /// is what the cache is intended to have in it.
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        List<IMarketingTest> GetTestList(TestCriteria criteria);

        /// <summary>
        /// Saves a test to the database.
        /// </summary>
        /// <param name="testObject">A test.</param>
        /// <returns></returns>
        Guid Save(IMarketingTest testObject);

        /// <summary>
        /// Removes a test from the database.
        /// </summary>
        /// <param name="testObjectId">Id of a test.</param>
        void Delete(Guid testObjectId);

        /// <summary>
        /// Starts a test.
        /// </summary>
        /// <param name="testObjectId">Id of a test.</param>
        void Start(Guid testObjectId);

        /// <summary>
        /// Stops a test.
        /// </summary>
        /// <param name="testObjectId">Id of a test.</param>
        void Stop(Guid testObjectId);

        /// <summary>
        /// Archives a test.
        /// </summary>
        /// <param name="testObjectId">Id of a test.</param>
        void Archive(Guid testObjectId, Guid winningVariantId);

        /// <summary>
        /// Saves a kpi result.  The result is appended to the list of results for a given variant version for a test for both historical and statistical calculations.
        /// </summary>
        /// <param name="testId">Id of a test to save the result to</param>
        /// <param name="itemVersion">Version of the variant the result pertains to.</param>
        /// <param name="keyResult">The result to save.</param>
        /// <param name="type">Type of the result to save.</param>
        /// <param name="aSynch">Boolean stating whether the result should be saved asynchronously or not.</param>
        void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type, bool aSynch = true);

        /// <summary>
        /// Increments a view or conversion by 1 for a given variant that is part of a given test.
        /// </summary>
        /// <param name="testId">Id of a test.</param>
        /// <param name="itemVersion">Version of the variant that should be updated.</param>
        /// <param name="kpiId">Id of the kpi count to increment.</param>
        /// /// <param name="resultType">Type of count to increment.</param>
        /// <param name="asynch">Boolean stating whether the result should be saved asynchronously or not.</param>
        void IncrementCount(Guid testId, int itemVersion, CountType resultType, bool asynch=true, Guid kpiId = default(Guid));

        /// <summary>
        /// Randomly decides if a new user is to be part of the ongoing test.  If so, chooses 1 of the 2 variants that are part of the test to display to the user.
        /// </summary>
        /// <param name="testId">Id of a test.</param>
        /// <returns></returns>
        Variant ReturnLandingPage(Guid testId);

        /// <summary>
        /// Gets the variant content from the variant cache.
        /// </summary>
        /// <param name="contentGuid">Id of the content to retrieve.</param>
        /// <returns></returns>
        IContent GetVariantContent(Guid contentGuid);

        /// <summary>
        /// Given a list of Kpi's and an EventArg object, each KPI will be evaluated and a list of Kpi instances 
        /// that have been evaluated will be returned.
        /// </summary>
        /// <param name="kpis"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e);

        /// <summary>
        /// If the database needs to be configured, then we return so that it can be set up.  If it has already been configured, we get the version of the current kpi schema and upgrade it if it is an older version.
        /// </summary>
        /// <param name="dbConnection">Connection properties for the desired database to connect to.</param>
        /// <param name="schema">Schema that should be applied to the database (upgrade or downgrade) if the database is outdated.</param>
        /// <param name="contextKey">The string used to identify the schema we are requesting the version of.</param>
        /// <param name="populateCache">If this is run before the site is set up, we want to populate the cache with all active tests.  By default, this is false.</param>
        /// <returns>Database version of the testing schema.</returns>
        long GetDatabaseVersion(DbConnection dbConnection, string schema, string contextKey, bool populateCache = false);

        /// <summary>
        /// Cache of all currently active tests.
        /// </summary>
        List<IMarketingTest> ActiveCachedTests { get; }
    }
}
