using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    public class CachingTestManager : ITestManager
    {
        private const string CacheValidityKey = "epi/marketing/testing/root";
        
        private readonly ITestManager _inner;
        private readonly ObjectCache _cache;
        private readonly ICacheSignal _remoteCacheSignal;
        private readonly DefaultMarketingTestingEvents _events;

        public CachingTestManager(ObjectCache cache, ICacheSignal remoteCacheSignal, DefaultMarketingTestingEvents events, ITestManager inner)
        {
            _remoteCacheSignal = remoteCacheSignal;
            _inner = inner;
            _events = events;
            _cache = cache;

            RefreshCache();

            remoteCacheSignal.Monitor(RefreshCache);
        }

        public void Archive(Guid testObjectId, Guid winningVariantId, CultureInfo cultureInfo = null)
        {
            _inner.Archive(testObjectId, winningVariantId, cultureInfo);
            RemoveFromCache(testObjectId);
        }

        public void Delete(Guid testObjectId, CultureInfo cultureInfo = null)
        {
            _inner.Delete(testObjectId, cultureInfo);
            RemoveFromCache(testObjectId);
        }

        public IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e)
        {
            return _inner.EvaluateKPIs(kpis, sender, e);
        }

        public IMarketingTest Get(Guid testObjectId, bool fromCache = false)
        {
            IMarketingTest test = null;

            if (fromCache)
            {
                test = (IMarketingTest)_cache.Get(GetCacheKeyForTest(testObjectId));
            }

            return test ?? _inner.Get(testObjectId, false);
        }

        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId)
        {
            return _cache
                .Where(test => test.Key.StartsWith($"epi/marketing/testing/tests?originalItem={originalItemId}"))
                .Select(test => test.Value as IMarketingTest)
                .Where(test => test.State == TestState.Active)
                .ToList();
        }

        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId, CultureInfo contentCulture)
        {
            return _cache
                .Where(test => test.Key.StartsWith(GetCacheKeyForTestByItem(originalItemId, contentCulture.Name)))
                .Select(test => test.Value as IMarketingTest)
                .Where(test => test.State == TestState.Active)
                .ToList();
        }

        public long GetDatabaseVersion(DbConnection dbConnection, string schema, string contextKey, bool populateCache = false)
        {
            return _inner.GetDatabaseVersion(dbConnection, schema, contextKey, populateCache);
        }

        public List<IMarketingTest> GetTestByItemId(Guid originalItemId)
        {
            return _inner.GetTestByItemId(originalItemId);
        }

        public List<IMarketingTest> GetTestList(TestCriteria criteria)
        {
            var cacheKey = GetCacheKeyForTests(criteria);
            var tests = _cache.Get(cacheKey) as List<IMarketingTest>;

            if(tests == null)
            {
                tests = _inner.GetTestList(criteria);

                if(tests?.Count() > 0)
                {
                    AddToCache(criteria, tests);
                }
            }

            return tests;
        }

        public IContent GetVariantContent(Guid contentGuid)
        {
            return GetVariantContent(contentGuid, new CultureInfo("en-GB"));
        }

        public IContent GetVariantContent(Guid contentGuid, CultureInfo cultureInfo)
        {
            IContent variant = null;

            variant = _cache.Get(GetCacheKeyForVariant(contentGuid, cultureInfo.Name)) as IContent;

            if (variant == null)
            {
                variant = _inner.GetVariantContent(contentGuid, cultureInfo);

                if (variant != null)
                {
                    AddToCache(contentGuid, cultureInfo, variant);
                }
            }

            return variant;
        }

        public void IncrementCount(IncrementCountCriteria criteria)
        {
            _inner.IncrementCount(criteria);
        }

        public void IncrementCount(Guid testId, int itemVersion, CountType resultType, Guid kpiId = default(Guid), bool asynch = true)
        {
            _inner.IncrementCount(testId, itemVersion, resultType, kpiId, asynch);
        }

        public Variant ReturnLandingPage(Guid testId)
        {
            return _inner.ReturnLandingPage(testId);
        }

        public Guid Save(IMarketingTest test)
        {
            var testId = _inner.Save(test);

            if(test.State == TestState.Active)
            {
                lock (_cache)
                {
                    AddToCache(test);
                }
            }

            return testId;
        }

        public void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type, bool isAsync = true)
        {
            _inner.SaveKpiResultData(testId, itemVersion, keyResult, type, isAsync);
        }

        public IMarketingTest Start(Guid testId)
        {
            var startedTest = _inner.Start(testId);

            if(startedTest?.State == TestState.Active)
            {
                lock (_cache)
                {
                    AddToCache(startedTest);
                }
            }

            return startedTest;
        }

        public void Stop(Guid testObjectId, CultureInfo cultureInfo = null)
        {
            _inner.Stop(testObjectId, cultureInfo);
            RemoveFromCache(testObjectId);
        }

        public void RefreshCache()
        {
            lock (_cache)
            {
                _cache.Where(test => test.Key.StartsWith("epi/marketing/testing/tests?id"))
                    .Cast<IMarketingTest>()
                    .Select(test => test.Id)
                    .ToList()
                    .ForEach(test => RemoveFromCache(test, false));

                var allActiveTests = new TestCriteria();
                allActiveTests.AddFilter(
                    new ABTestFilter
                    {
                        Property = ABTestProperty.State,
                        Operator = FilterOperator.And,
                        Value = TestState.Active
                    }
                );

                _inner.GetTestList(allActiveTests).ForEach(test => AddToCache(test, false));

                _remoteCacheSignal.Set();
            }
        }

        private void AddToCache(IMarketingTest test)
        {
            AddToCache(test, true);
        }

        private void AddToCache(IMarketingTest test, bool invalidateRemote)
        {            
            // Adds the test and dependent entries to the cache:
            //   test (root)
            //    |
            //     -- test (by original item)
            
            var testCacheKey = GetCacheKeyForTest(test.Id);
            _cache.Add(testCacheKey, test, new CacheItemPolicy());
            _cache.Add(GetCacheKeyForTestByItem(test.OriginalItemId, test.ContentLanguage), test, GetCachePolicyForTest(test, testCacheKey));

            // Adding a test to the cache potentially invalidates lists of tests
            // that were previously stored. So, remove them all.

            _cache.Where(t => t.Key.StartsWith($"epi/marketing/testing/tests?filter="))
                .Select(t => t.Key)
                .ToList()
                .ForEach(key => _cache.Remove(key));
            
            // Notify interested consumers that a test was added to the cache.

            _events.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestAddedToCacheEvent, new TestEventArgs(test));

            // Signal other nodes to reset their cache.

            if (invalidateRemote)
            {
                _remoteCacheSignal.Reset();
            }
        }

        private void AddToCache(TestCriteria criteria, IEnumerable<IMarketingTest> tests)
        {
            // Adds a list of tests to the cache. The list is dependent on all tests
            // it contains so that it will be invalidated if one of those tests should
            // change.
            // 
            //  test    test    test
            //   |       |       |
            //    ---------------
            //           |
            //          list

            // Add the individual tests to the cache.

            List<string> dependencies = new List<string>();
            foreach(var test in tests)
            {
                AddToCache(test);
                dependencies.Add(GetCacheKeyForTest(test.Id));
            }

            // Add the list to the cache and make it dependent on all of its children

            var policy = new CacheItemPolicy();

            if (dependencies.Any())
            {
                policy.ChangeMonitors.Add(_cache.CreateCacheEntryChangeMonitor(dependencies));
            }

            _cache.Add(GetCacheKeyForTests(criteria), tests, policy);
        }

        private void AddToCache(Guid originalItemId, CultureInfo culture, IContent variant)
        {
            // Adds a variant to the cache. The variant is dependent on its parent test
            // so that it will be invalidated if its parent should change.
            //
            //   test (root)
            //    |
            //     -- test (by original item)
            //         |
            //          -- variant

            var cacheKeyForVariant = GetCacheKeyForVariant(originalItemId, culture.Name);
            var cacheKeyForAssociatedTest = GetCacheKeyForTestByItem(originalItemId, culture.Name);
            var policy = GetCachePolicyForVariant(originalItemId, culture, variant, cacheKeyForAssociatedTest);

            _cache.Add(cacheKeyForVariant, variant, policy);
        }

        private void RemoveFromCache(Guid testId)            
        {
            RemoveFromCache(testId, true);
        }

        private void RemoveFromCache(Guid testId, bool invalidateRemote)
        {
            var removedTest = _cache.Remove(GetCacheKeyForTest(testId)) as IMarketingTest;

            if(invalidateRemote && removedTest != null)
            {
                _remoteCacheSignal.Reset();
            }
        }

        private CacheItemPolicy GetCachePolicyForTest(IMarketingTest test, params string[] dependencies)
        {
            var policy = new CacheItemPolicy()
            {
                AbsoluteExpiration = ObjectCache.InfiniteAbsoluteExpiration,
                RemovedCallback = args => _events.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, new TestEventArgs(test))
            };

            if (dependencies.Any())
            {
                policy.ChangeMonitors.Add(_cache.CreateCacheEntryChangeMonitor(dependencies));
            }

            return policy;
        }

        private CacheItemPolicy GetCachePolicyForVariant(Guid originalItemId, CultureInfo culture, IContent variantContent, params string[] dependencies)
        {
            var policy = new CacheItemPolicy();

            if (dependencies.Any())
            {
                policy.ChangeMonitors.Add(_cache.CreateCacheEntryChangeMonitor(dependencies));
            }

            return policy;
        }

        private static string GetCacheKeyForVariant(Guid contentGuid, string contentLanguage)
        {
            return $"epi/marketing/testing/variants?originalItem={contentGuid}&culture={contentLanguage}";
        }

        private static string GetCacheKeyForTest(Guid id)
        {
            return $"epi/marketing/testing/tests?id={id}";
        }

        private static string GetCacheKeyForTests(TestCriteria criteria)
        {
            var query = string.Join(
                "&", 
                criteria.GetFilters().Select(f => $"filter={f.Property.ToString()} {f.Operator.ToString()} {f.Value?.ToString() ?? ""}")
            );

            return $"epi/marketing/testing/tests?{query}";
        }

        private static string GetCacheKeyForTestByItem(Guid originalItemId, string contentCulture)
        {
            return $"epi/marketing/testing/tests?originalItem={originalItemId}&culture={contentCulture}";
        }
    }
}
