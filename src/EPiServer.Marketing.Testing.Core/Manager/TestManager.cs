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
using System.Globalization;
using EPiServer.Framework.Cache;
using System.Threading.Tasks;

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
        internal const string CacheValidFlag = "CacheValidFlag";

        private static Object _cacheLock = new object();
        private ITestingDataAccess _dataAccess;
        private IServiceLocator _serviceLocator;
        private Random _randomParticiaption = new Random();
        private ObjectCache _testCache = MemoryCache.Default;
        private ISynchronizedObjectInstanceCache _testCacheValidFlag;
        private ObjectCache _variantCache = MemoryCache.Default;
        private IKpiManager _kpiManager;
        private DefaultMarketingTestingEvents _marketingTestingEvents;

        public bool DatabaseNeedsConfiguring;

        /// <inheritdoc />
        public List<IMarketingTest> ActiveCachedTests
        {
            get
            {
                ManageCaches(); // MAR-904 - make sure that there is always a cache
                                // MAR-1192 - Load Balanced environments causeing internal exceptions randomly.
                return _testCache.Get(TestingCacheName) as List<IMarketingTest>;
            }
        }

        [ExcludeFromCodeCoverage]
        public TestManager()
        {
            _serviceLocator = ServiceLocator.Current;
            _marketingTestingEvents = _serviceLocator.GetInstance<DefaultMarketingTestingEvents>();
            _testCacheValidFlag = _serviceLocator.GetInstance<ISynchronizedObjectInstanceCache>();

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
            _testCacheValidFlag = _serviceLocator.GetInstance<ISynchronizedObjectInstanceCache>();
        }

        /// <summary>
        /// Responsible for initializing the test cache as well as managing it in load balanced environments. Note that when a content authoring machines
        /// signal that the cache is out of date (by removing the CacheValidFlag from all content delivery machines this code will reload all the tests
        /// and add them to the cache.
        /// </summary>
        private void ManageCaches()
        {
            var cacheValidFlag = _testCacheValidFlag.Get(CacheValidFlag);
            if (cacheValidFlag == null || !_testCache.Contains(TestingCacheName))
            {
                // Cache is either out of date or doesnt exist yet.
                lock (_cacheLock)
                {
                    cacheValidFlag = _testCacheValidFlag.Get(CacheValidFlag);
                    if (cacheValidFlag == null || !_testCache.Contains(TestingCacheName))
                    {
                        // Clear the variant cache
                        var allKeys = _variantCache.Where(o => o.Key.Contains("epi")).Select(k => k.Key);
                        Parallel.ForEach(allKeys, key => _variantCache.Remove(key));

                        UpdateActiveTestCache();

                        // Insert the flag so that we know that we know this server is up to date. 
                        _testCacheValidFlag.Insert(CacheValidFlag, "true", CacheEvictionPolicy.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Clears the current test cache and reloads it.
        /// </summary>
        private void UpdateActiveTestCache()
        {
            // Clear the test cache, fire the TestRemovedFromCacheEvent to disable the event proxy for kpis.
            if (_testCache.Contains(TestingCacheName))
            {
                foreach (var test in (List<IMarketingTest>)_testCache.Get(TestingCacheName))
                {
                    _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, new TestEventArgs(test));
                }
                _testCache.Remove(TestingCacheName);
            }

            // Get all the tests that are supposed to be active and add to cache.
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

            // now for every test added, fire the TestAddedToCacheEvent.
            // Note that this event is also used to intialize the event proxy for kpis
            foreach (var test in tests)
            {
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestAddedToCacheEvent, new TestEventArgs(test));
            }
        }

        /// <inheritdoc />
        public IMarketingTest Get(Guid testObjectId, bool fromCachedTests = false)
        {
            IMarketingTest retrievedTest = null;

            if (fromCachedTests)
            {
                //Will attempt to retrieve a test from the cache.  If unsuccessful will then
                //retrieve the test from the db

                retrievedTest = ActiveCachedTests.Where(test => test.Id == testObjectId).FirstOrDefault();

                if (retrievedTest == null)
                {
                    retrievedTest = Get(testObjectId, false);
                }
            }
            else
            {
                retrievedTest = TestManagerHelper.ConvertToManagerTest(_kpiManager, _dataAccess.Get(testObjectId));

                if (retrievedTest == null)
                {
                    throw new TestNotFoundException();
                }
            }

            return retrievedTest;
        }
        
        /// <inheritdoc />
        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId)
        {
            var cachedTests = ActiveCachedTests;
            return cachedTests.Where(test => test.OriginalItemId == originalItemId).ToList();
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId,CultureInfo contentCulture)
        {
            var cachedTests = ActiveCachedTests;
            return cachedTests.Where(test => test.OriginalItemId == originalItemId && test.ContentLanguage == contentCulture.Name).ToList();
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
        /// <returns>ID of the test.</returns>
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
        public void Delete(Guid testObjectId, CultureInfo cultureInfo = null)
        {
            var testToDelete = Get(testObjectId);
            RemoveCachedVariant(testToDelete.OriginalItemId, cultureInfo);

            foreach (var kpi in testToDelete.KpiInstances)
            {
                _kpiManager.Delete(kpi.Id);
            }

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
        public IMarketingTest Start(Guid testObjectId)
        {
            var dalTest = _dataAccess.Start(testObjectId);
            var managerTest = TestManagerHelper.ConvertToManagerTest(_kpiManager, dalTest);

            // update cache to include new test as long as it was changed to Active
            if (dalTest != null)
            {
                UpdateCache(managerTest, CacheOperator.Add);
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStartedEvent, new TestEventArgs(managerTest));
            }

            return managerTest;
        }

        /// <inheritdoc />
        public void Stop(Guid testObjectId, CultureInfo cultureInfo = null)
        {
            _dataAccess.Stop(testObjectId);

            RemoveCachedVariant(Get(testObjectId).OriginalItemId, cultureInfo);

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
        public void Archive(Guid testObjectId, Guid winningVariantId, CultureInfo cultureInfo = null)
        {
            _dataAccess.Archive(testObjectId, winningVariantId);
            RemoveCachedVariant(Get(testObjectId).OriginalItemId, cultureInfo);
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

            return retData ?? UpdateVariantContentCache(contentGuid, new CultureInfo("en-GB"));
        }

        /// <inheritdoc />
        public IContent GetVariantContent(Guid contentGuid, CultureInfo cultureInfo)
        {
            var retData = (IContent)_variantCache.Get("epi" + contentGuid + ":" + cultureInfo.Name);

            return retData ?? UpdateVariantContentCache(contentGuid, cultureInfo);
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
                ManageCaches();
            }

            return _dataAccess.GetDatabaseVersion(dbConnection, schema, contextKey);
        }


        public void UpdateCache(IMarketingTest test, CacheOperator cacheOperator)
        {
            var cachedTests = ActiveCachedTests;

            switch (cacheOperator)
            {
                case CacheOperator.Add:
                    if (!cachedTests.Contains(test))
                    {
                        cachedTests.Add(test);
                        _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestAddedToCacheEvent,new TestEventArgs(test));

                        _testCacheValidFlag.RemoveRemote(CacheValidFlag);
                    }
                    break;
                case CacheOperator.Remove:
                    if (cachedTests.Contains(test))
                    {
                        cachedTests.Remove(test);
                        _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent,new TestEventArgs(test));

                        _testCacheValidFlag.RemoveRemote(CacheValidFlag);
                    }
                    break;
            }
        }

        internal void RemoveCachedVariant(Guid contentGuid, CultureInfo cultureInfo)
        {
            if (null != cultureInfo)
            {
                if (_variantCache.Contains("epi" + contentGuid + ":" + cultureInfo.Name))
                {
                    _variantCache.Remove("epi" + contentGuid + ":" + cultureInfo.Name);
                }
            }
            else
            {
                if (_variantCache.Contains("epi" + contentGuid))
                {
                    _variantCache.Remove("epi" + contentGuid);
                }
            }
        }

        internal IContent UpdateVariantContentCache(Guid contentGuid, CultureInfo cultureInfo)
        {
            IVersionable versionableContent = null;

            var test =
                GetActiveTestsByOriginalItemId(contentGuid, cultureInfo).FirstOrDefault(x => x.State.Equals(TestState.Active));

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

                            _variantCache.Add("epi" + contentGuid + ":" + cultureInfo.Name, (IContent)versionableContent, cacheItemPolicy);
                        }
                    }
                }
            }
            return (IContent)versionableContent;
        }
    }
}
