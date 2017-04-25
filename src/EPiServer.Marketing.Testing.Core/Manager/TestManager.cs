using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Caching;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Exceptions;
using EPiServer.Marketing.Testing.Dal.DataAccess;
using EPiServer.Marketing.Testing.Dal.Exceptions;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// Used to say what operation should be done to the cache.
    /// </summary>
    public enum CacheOperator
    {
        Add,
        Remove
    }

    /// <summary>
    /// Central point of access for test data and test manipulation.
    /// </summary>
    [ServiceConfiguration(ServiceType = typeof(ITestManager), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TestManager : ITestManager
    {
        internal const string TestingCacheName = "TestingCache";
        private ITestingDataAccess _dataAccess;
        private IServiceLocator _serviceLocator;
        private Random _randomParticiaption = new Random();
        private ObjectCache _testCache = MemoryCache.Default;
        private ObjectCache _variantCache = MemoryCache.Default;
        private IKpiManager _kpiManager;
        private DefaultMarketingTestingEvents _marketingTestingEvents;

        public bool DatabaseNeedsConfiguring;

        /// <inheritdoc />
        public List<IMarketingTest> ActiveCachedTests
        {
            get
            {
                initCache(); // MAR-904 - make sure that there is always a cache
                return _testCache.Get(TestingCacheName) as List<IMarketingTest>;
            }
        }

        [ExcludeFromCodeCoverage]
        public TestManager()
        {
            _serviceLocator = ServiceLocator.Current;
            _marketingTestingEvents = ServiceLocator.Current.GetInstance<DefaultMarketingTestingEvents>();

            try
            {
                _dataAccess = new TestingDataAccess();
            }
            catch (DatabaseDoesNotExistException)
            {
                DatabaseNeedsConfiguring = true;
                return;
            }
            catch (DatabaseNeedsUpdating)
            {
                DatabaseNeedsConfiguring = true;
                return;
            }

            _kpiManager = new KpiManager();
        }

        internal TestManager(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _dataAccess = _serviceLocator.GetInstance<ITestingDataAccess>();
            _kpiManager = _serviceLocator.GetInstance<IKpiManager>();
            _marketingTestingEvents = _serviceLocator.GetInstance<DefaultMarketingTestingEvents>();
        }

        private void initCache()
        {
            if (!_testCache.Contains(TestingCacheName))
            {
                var activeTestCriteria = new TestCriteria();
                var activeTestStateFilter = new ABTestFilter()
                {
                    Property = ABTestProperty.State,
                    Operator = FilterOperator.And,
                    Value = TestState.Active
                };

                activeTestCriteria.AddFilter(activeTestStateFilter);

                var tests = GetTestList(activeTestCriteria);
                _testCache.Add(TestingCacheName, tests, DateTimeOffset.MaxValue);
            }
        }

        /// <inheritdoc />
        public IMarketingTest Get(Guid testObjectId)
        {
            var dbTest = _dataAccess.Get(testObjectId);
            if (dbTest == null)
            {
                throw new TestNotFoundException();
            }

            return TestManagerHelper.ConvertToManagerTest(_kpiManager, dbTest);
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId)
        {
            var cachedTests = ActiveCachedTests;
            return cachedTests.Where(test => test.OriginalItemId == originalItemId).ToList();
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetTestByItemId(Guid originalItemId)
        {
            var testList = new List<IMarketingTest>();

            foreach (var dalTest in _dataAccess.GetTestByItemId(originalItemId))
            {
                testList.Add(TestManagerHelper.ConvertToManagerTest(_kpiManager, dalTest));
            }
            return testList;
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetTestList(TestCriteria criteria)
        {
            var testList = new List<IMarketingTest>();

            foreach (var dalTest in _dataAccess.GetTestList(TestManagerHelper.ConvertToDalCriteria(criteria)))
            {
                testList.Add(TestManagerHelper.ConvertToManagerTest(_kpiManager, dalTest));
            }
            return testList;
        }

        /// <summary>
        /// Saves a test to the database.
        /// </summary>
        /// <param name="multivariateTest">A test.</param>
        /// <returns>Id of the test.</returns>
        public Guid Save(IMarketingTest multivariateTest)
        {
            // need to check that the list isn't null before checking for actual kpi's so we don't get a null reference exception
            if (multivariateTest.KpiInstances == null)
            {
                throw new SaveTestException("Unable to save test due to null list of KPI's.  One or more KPI's are required.");
            }

            if (multivariateTest.KpiInstances.Count == 0)
            {
                throw new SaveTestException("Unable to save test due to empty list of KPI's.  One or more KPI's are required.");
            }

            var testId = _dataAccess.Save(TestManagerHelper.ConvertToDalTest(multivariateTest));
            _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestSavedEvent, new TestEventArgs(multivariateTest));

            if (multivariateTest.State == TestState.Active)
            {
                UpdateCache(multivariateTest, CacheOperator.Add);
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStartedEvent, new TestEventArgs(multivariateTest));
            }

            return testId;
        }

        /// <inheritdoc />
        public void Delete(Guid testObjectId)
        {
            RemoveCachedVariant(Get(testObjectId).OriginalItemId);

            _dataAccess.Delete(testObjectId);

            // if the test is in the cache remove it.  This should only happen if someone deletes an Active test - which really shouldn't happen...
            var cachedTests = ActiveCachedTests;
            var test = cachedTests.FirstOrDefault(t => t.Id == testObjectId);

            if (test != null)
            {
                UpdateCache(test, CacheOperator.Remove);
            }

            _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestDeletedEvent, new TestEventArgs(test));

        }

        /// <inheritdoc />
        public void Start(Guid testObjectId)
        {
            var dalTest = _dataAccess.Start(testObjectId);
            var managerTest = TestManagerHelper.ConvertToManagerTest(_kpiManager, dalTest);

            // update cache to include new test as long as it was changed to Active
            if (dalTest != null)
            {
                UpdateCache(managerTest, CacheOperator.Add);
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStartedEvent, new TestEventArgs(managerTest));
            }
        }

        /// <inheritdoc />
        public void Stop(Guid testObjectId)
        {
            _dataAccess.Stop(testObjectId);

            RemoveCachedVariant(Get(testObjectId).OriginalItemId);

            var cachedTests = ActiveCachedTests;

            // remove test from cache
            var test = cachedTests.FirstOrDefault(x => x.Id == testObjectId);
            if (test != null)
            {
                UpdateCache(test, CacheOperator.Remove);
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStoppedEvent, new TestEventArgs(test));
            }

        }

        /// <inheritdoc />
        public void Archive(Guid testObjectId, Guid winningVariantId)
        {
            _dataAccess.Archive(testObjectId, winningVariantId);
            RemoveCachedVariant(Get(testObjectId).OriginalItemId);
            var cachedTests = ActiveCachedTests;
            var test = cachedTests.FirstOrDefault(x => x.Id == testObjectId);
            if (test != null)
            {
                UpdateCache(test,CacheOperator.Remove);
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestArchivedEvent, new TestEventArgs(test));
            }

        }

        /// <inheritdoc />
        public void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type, bool aSynch = true)
        {
            if (aSynch)
            {
                var messaging = _serviceLocator.GetInstance<IMessagingManager>();
                messaging.EmitKpiResultData(testId, itemVersion, keyResult, type);
            }
            else
            {
                switch (type)
                {
                    case KeyResultType.Financial:
                        _dataAccess.AddKpiResultData(testId, itemVersion, TestManagerHelper.ConvertToDalKeyFinancialResult((KeyFinancialResult)keyResult), (int)type);
                        break;
                    case KeyResultType.Value:
                        _dataAccess.AddKpiResultData(testId, itemVersion,
                            TestManagerHelper.ConvertToDalKeyValueResult((KeyValueResult) keyResult), (int) type);
                        break;
                    case KeyResultType.Conversion:
                    default:
                        _dataAccess.AddKpiResultData(testId, itemVersion,
                            TestManagerHelper.ConvertToDalKeyConversionResult((KeyConversionResult)keyResult), (int)type);
                        break;
                }
            }
        }

        /// <inheritdoc />
        public Variant ReturnLandingPage(Guid testId)
        {
            var currentTest = _dataAccess.Get(testId);
            var managerTest = TestManagerHelper.ConvertToManagerTest(_kpiManager, currentTest);
            var activePage = new Variant();
            if (managerTest != null)
            {
                if (_randomParticiaption.Next(1, 100) <= managerTest.ParticipationPercentage)
                {
                    switch (TestManagerHelper.GetRandomNumber())
                    {
                        case 1:
                        default:
                            activePage = TestManagerHelper.ConvertToManagerVariant(currentTest.Variants[0]);
                            break;
                        case 2:
                            activePage = TestManagerHelper.ConvertToManagerVariant(currentTest.Variants[1]);
                            break;
                    }
                    _marketingTestingEvents.
                        RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.UserIncludedInTestEvent,
                        new TestEventArgs(managerTest));
                }
            }
            return activePage;
        }

        /// <inheritdoc />
        public IContent GetVariantContent(Guid contentGuid)
        {
            var retData = (IContent)_variantCache.Get("epi" + contentGuid);

            return retData ?? UpdateVariantContentCache(contentGuid);
        }

        private Object _incrementLock = new Object();
        /// <inheritdoc />
        public void IncrementCount(IncrementCountCriteria criteria)
        {
            if (criteria.asynch)
            {
                var messaging = _serviceLocator.GetInstance<IMessagingManager>();
                if (criteria.resultType == CountType.Conversion)
                    messaging.EmitUpdateConversion(criteria.testId, criteria.itemVersion, criteria.kpiId, criteria.clientId);
                else if (criteria.resultType == CountType.View)
                    messaging.EmitUpdateViews(criteria.testId, criteria.itemVersion);
            }
            else
            {
                lock (_incrementLock)
                {
                    _dataAccess.IncrementCount(criteria.testId, criteria.itemVersion, TestManagerHelper.AdaptToDalCount(criteria.resultType), criteria.kpiId);
                }
            }
        }

        /// <inheritdoc />
        public void IncrementCount(Guid testId, int itemVersion, CountType resultType, Guid kpiId = default(Guid), bool asynch = true)
        {
            var c = new IncrementCountCriteria()
            {
                testId = testId,
                itemVersion = itemVersion,
                resultType = resultType,
                kpiId = kpiId,
                asynch = asynch
            };
            IncrementCount(c);
        }

        /// <inheritdoc />
        public IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e)
        {
            return kpis.Select(kpi => kpi.Evaluate(sender, e)).ToList();
        }

        /// <inheritdoc />
        public long GetDatabaseVersion(DbConnection dbConnection, string schema, string contextKey, bool populateCache = false)
        {
            if (DatabaseNeedsConfiguring)
            {
                DatabaseNeedsConfiguring = false;
                return 0;
            }

            if (populateCache)
            {
                _dataAccess = new TestingDataAccess();
                _kpiManager = new KpiManager();
                initCache();
            }

            return _dataAccess.GetDatabaseVersion(dbConnection, schema, contextKey);
        }


        internal void UpdateCache(IMarketingTest test, CacheOperator cacheOperator)
        {
            var cachedTests = ActiveCachedTests;

            switch (cacheOperator)
            {
                case CacheOperator.Add:
                    if (!cachedTests.Contains(test))
                    {
                        cachedTests.Add(test);
                        _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestAddedToCacheEvent,new TestEventArgs(test));
                    }
                    break;
                case CacheOperator.Remove:
                    if (cachedTests.Contains(test))
                    {
                        cachedTests.Remove(test);
                        _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent,new TestEventArgs(test));
                    }
                    break;
            }
        }

        internal void RemoveCachedVariant(Guid contentGuid)
        {
            if (_variantCache.Contains("epi" + contentGuid))
            {
                _variantCache.Remove("epi" + contentGuid);
            }
        }

        internal IContent UpdateVariantContentCache(Guid contentGuid)
        {
            IVersionable versionableContent = null;

            var test =
                GetActiveTestsByOriginalItemId(contentGuid).FirstOrDefault(x => x.State.Equals(TestState.Active));

            if (test != null)
            {
                var contentLoader = _serviceLocator.GetInstance<IContentLoader>();
                var testContent = contentLoader.Get<IContent>(contentGuid);
                var contentVersion = testContent.ContentLink.WorkID == 0
                    ? test.Variants.First(variant => variant.IsPublished).ItemVersion
                    : testContent.ContentLink.WorkID;

                if (testContent != null)
                {
                    foreach (var variant in test.Variants)
                    {
                        if (variant.ItemVersion != contentVersion)
                        {
                            versionableContent = (IVersionable)TestManagerHelper.CreateVariantContent(contentLoader, testContent, variant);
                            versionableContent.Status = VersionStatus.Published;
                            versionableContent.StartPublish = DateTime.Now.AddDays(-1);

                            var cacheItemPolicy = new CacheItemPolicy
                            {
                                AbsoluteExpiration = DateTimeOffset.Parse(test.EndDate.ToString())
                            };

                            _variantCache.Add("epi" + contentGuid, (IContent)versionableContent, cacheItemPolicy);
                        }
                    }
                }
            }
            return (IContent)versionableContent;
        }
    }
}
