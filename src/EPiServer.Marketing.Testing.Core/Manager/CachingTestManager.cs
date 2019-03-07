using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    public class CachingTestManager : ITestManager
    {
        private readonly ISynchronizedObjectInstanceCache _cache;
        private readonly IContentLoader _contentLoader;
        private readonly ITestManager _inner;

        public CachingTestManager(ISynchronizedObjectInstanceCache cache, IContentLoader contentLoader, ITestManager inner)
        {
            _cache = cache;
            _contentLoader = contentLoader;
            _inner = inner;
        }

        public List<IMarketingTest> ActiveCachedTests => throw new NotImplementedException();

        public void Archive(Guid testObjectId, Guid winningVariantId, CultureInfo cultureInfo = null)
        {
            _inner.Archive(testObjectId, winningVariantId, cultureInfo);
            _cache.Remove(GetCacheKeyForTest(testObjectId));            
        }

        public void Delete(Guid testObjectId, CultureInfo cultureInfo = null)
        {
            _inner.Delete(testObjectId, cultureInfo);
            _cache.Remove(GetCacheKeyForTest(testObjectId));
        }

        public IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e)
        {
            return _inner.EvaluateKPIs(kpis, sender, e);
        }

        public IMarketingTest Get(Guid testObjectId, bool fromCache = false)
        {
            IMarketingTest test = null;

            _cache.TryGet(GetCacheKeyForTest(testObjectId), ReadStrategy.Immediate, out test);

            if (test == null)
            {
                test = _inner.Get(testObjectId);

                if(test != null)
                {
                    AddToCache(test);
                }
            }

            return test;
        }

        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId)
        {
            return GetTestByItemId(originalItemId)
                .Where(t => t.State == TestState.Active)
                .ToList();
        }

        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId, CultureInfo contentCulture)
        {
            return GetTestByItemId(originalItemId)
                .Where(t => t.State == TestState.Active && t.ContentLanguage == contentCulture.Name)
                .ToList();
        }

        public long GetDatabaseVersion(DbConnection dbConnection, string schema, string contextKey, bool populateCache = false)
        {
            return _inner.GetDatabaseVersion(dbConnection, schema, contextKey, populateCache);
        }

        public List<IMarketingTest> GetTestByItemId(Guid originalItemId)
        {
            var cacheKey = GetCacheKeyForTestsByItem(originalItemId);

            List<IMarketingTest> tests = null;

            _cache.TryGet(cacheKey, ReadStrategy.Immediate, out tests);

            if (tests == null)
            {
                tests = _inner.GetTestByItemId(originalItemId);

                if (tests != null && tests.Any())
                {
                    tests.ForEach(AddToCache);
                    _cache.Insert(cacheKey, tests, new CacheEvictionPolicy(tests.Select(t => GetCacheKeyForTest(t.Id))));
                }
            }

            return tests ?? new List<IMarketingTest>();
        }

        public List<IMarketingTest> GetTestList(TestCriteria criteria)
        {
            return _inner.GetTestList(criteria);
        }

        public IContent GetVariantContent(Guid contentGuid)
        {
            return GetVariantContent(contentGuid, new CultureInfo("en-GB"));
        }

        public IContent GetVariantContent(Guid contentGuid, CultureInfo cultureInfo)
        {
            IContent variant = null;

            _cache.TryGet(GetCacheKeyForVariant(contentGuid, cultureInfo.Name), ReadStrategy.Immediate, out variant);

            if (variant == null)
            {
                variant = _inner.GetVariantContent(contentGuid, cultureInfo);
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
                _cache.Insert(GetCacheKeyForTest(testId), test, CacheEvictionPolicy.Empty);
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
                _cache.Insert(GetCacheKeyForTest(testId), startedTest, CacheEvictionPolicy.Empty);
            }

            return startedTest;
        }

        public void Stop(Guid testObjectId, CultureInfo cultureInfo = null)
        {
            _inner.Stop(testObjectId, cultureInfo);
            _cache.Remove(GetCacheKeyForTest(testObjectId));
        }

        public void UpdateCache(IMarketingTest test, CacheOperator cacheOperator)
        {
            throw new NotImplementedException();
        }

        private void AddToCache(IMarketingTest test)
        {
            var testCacheKey = GetCacheKeyForTest(test.Id);

            _cache.Insert(testCacheKey, test, CacheEvictionPolicy.Empty);

            _cache.Insert(
                GetCacheKeyForVariant(test.OriginalItemId, test.ContentLanguage),
                _inner.GetVariantContent(test.OriginalItemId, new CultureInfo(test.ContentLanguage)),
                new CacheEvictionPolicy(new[] { testCacheKey })
            );

            _cache.Remove(GetCacheKeyForTestsByItem(test.OriginalItemId));
        }

        private static string GetCacheKeyForVariant(Guid contentGuid, string contentLanguage)
        {
            return $"epi/marketing/testing/variants?originalItem={contentGuid}&culture={contentLanguage}";
        }

        private static string GetCacheKeyForTest(Guid id)
        {
            return $"epi/marketing/testing/tests?id={id}";
        }

        private static string GetCacheKeyForTestsByItem(Guid originalItemId)
        {
            return $"epi/marketing/testing/tests?originalItem={originalItemId}";
        }
    }
}
