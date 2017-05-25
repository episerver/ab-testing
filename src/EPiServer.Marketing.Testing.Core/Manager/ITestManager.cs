using System;
using System.Collections.Generic;
using System.Data.Common;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using System.Globalization;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// Central point of access for test data and test manipulation.
    /// </summary>
    public interface ITestManager
    {
        /// <summary>
        /// Gets a test based on the supplied id from the database.
        /// </summary>
        /// <param name="testObjectId">ID of the test to retrieve.</param>
        /// <returns>IMarketing Test</returns>
        IMarketingTest Get(Guid testObjectId);

        /// <summary>
        /// Retrieves all active tests that have the supplied OriginalItemId from the cache.  The associated data for each 
        /// test returned may not be current.  If the most current data is required 'Get' should be used instead.
        /// </summary>
        /// <param name="originalItemId">ID of the item under test.</param>
        /// <returns>List of tests.</returns>
        List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId);


        /// <summary>
        /// Retrieves all active tests that have the supplied OriginalItemId and ContentCulture name. The associated data for each
        /// test returned may not be current.  If the most current data is required 'Get' should be used instead.
        /// </summary>
        /// <param name="originalItemId">ID of the item under test.</param>
        /// <param name="contentCulture">Content Culture of the current loaded content</param>
        /// <returns></returns>
        List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId, CultureInfo contentCulture);


        /// <summary>
        /// Retrieves all tests, regardless of test state, that have the supplied OriginalItemId.
        /// </summary>
        /// <param name="originalItemId">ID of the item under test.</param>
        /// <returns>List of tests.</returns>
        List<IMarketingTest> GetTestByItemId(Guid originalItemId);

        /// <summary>
        /// Don't want to use refernce the cache here.  The criteria could be anything, not just active tests which
        /// is what the cache is intended to have in it.
        /// </summary>
        /// <param name="criteria">A group of filters that are applied to the list of tests in order to limit what is returned.</param>
        /// <returns>A list of tests based on the criteria passed in.</returns>
        List<IMarketingTest> GetTestList(TestCriteria criteria);

        /// <summary>
        /// Saves a test to the database.
        /// </summary>
        /// <param name="testObject">A test.</param>
        /// <returns>ID of the test.</returns>
        Guid Save(IMarketingTest testObject);

        /// <summary>
        /// Removes a test from the database.
        /// </summary>
        /// <param name="testObjectId">ID of a test.</param>
        void Delete(Guid testObjectId);

        /// <summary>
        /// Starts a test.
        /// </summary>
        /// <param name="testObjectId">ID of a test.</param>
        void Start(Guid testObjectId);

        /// <summary>
        /// Stops a test.
        /// </summary>
        /// <param name="testObjectId">ID of a test.</param>
        void Stop(Guid testObjectId);

        /// <summary>
        /// Archives a test.
        /// </summary>
        /// <param name="testObjectId">ID of a test.</param>
        /// <param name="winningVariantId">ID of the variant that was declared the winner.</param>
        void Archive(Guid testObjectId, Guid winningVariantId);

        /// <summary>
        /// Saves a KPI result.  The result is appended to the list of results for a given variant version for a test for both historical and statistical calculations.
        /// </summary>
        /// <param name="testId">ID of a test to save the result to.</param>
        /// <param name="itemVersion">Version of the variant the result pertains to.</param>
        /// <param name="keyResult">The result to save.</param>
        /// <param name="type">Type of the result to save.</param>
        /// <param name="aSynch">Boolean stating whether the result should be saved asynchronously or not.</param>
        void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type, bool aSynch = true);

        /// <summary>
        /// Increments a view or conversion by 1 for a given variant that is part of a given test.
        /// </summary>
        /// <param name="criteria">Critera class to define what to increment.</param>
        void IncrementCount(IncrementCountCriteria criteria);

        /// <summary>
        /// Increments a view or conversion by 1 for a given variant that is part of a given test.
        /// </summary>
        /// <param name="testId">ID of a test.</param>
        /// <param name="itemVersion">Version of the variant that should be updated.</param>
        /// <param name="kpiId">ID of the KPI count to increment.</param>
        /// /// <param name="resultType">Type of count to increment.</param>
        /// <param name="asynch">Boolean stating whether the result should be saved asynchronously or not.</param>
        void IncrementCount(Guid testId, int itemVersion, CountType resultType, Guid kpiId = default(Guid), bool asynch=true);

        /// <summary>
        /// Randomly decides if a new user is to be part of the ongoing test.  If so, chooses 1 of the 2 variants that are part of the test to display to the user.
        /// </summary>
        /// <param name="testId">ID of a test.</param>
        /// <returns>The original or variant item under test.</returns>
        Variant ReturnLandingPage(Guid testId);

        /// <summary>
        /// Gets the variant content from the variant cache.
        /// </summary>
        /// <param name="contentGuid">ID of the content to retrieve.</param>
        /// <returns>A content item.</returns>
        IContent GetVariantContent(Guid contentGuid);

        /// <summary>
        /// Given a list of KPIs and an EventArg object, each KPI will be evaluated and a list of KPI instances 
        /// that have been evaluated will be returned.
        /// </summary>
        /// <param name="kpis">List of KPIs to evaluate for conersion.</param>
        /// <param name="sender">Sender of the event that pertains to the KPI.</param>
        /// <param name="e">Args associated with the KPIs.</param>
        /// <returns></returns>
        IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e);

        /// <summary>
        /// If the database needs to be configured, then we return so that it can be set up.  If it has already been configured, we get the version of the current KPI schema and upgrade it if it is an older version.
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
