using System;
using System.Collections.Generic;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Security;
using EPiServer.Core;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IMarketingTestingWebRepository))]
    public class MarketingTestingWebRepository : IMarketingTestingWebRepository
    {
        private IServiceLocator _serviceLocator;

        /// <summary>
        /// Default constructor
        /// </summary>
        [ExcludeFromCodeCoverage]
        public MarketingTestingWebRepository()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        /// <summary>
        /// Gets the test associated with the content guid specified. If no tests are found an empty test is returned
        /// </summary>
        /// <param name="aContentGuid">the content guid to search against</param>
        /// <returns>the first marketing test found that is not done or archived or an empty test in the case of no results</returns>
        public IMarketingTest GetActiveTestForContent(Guid aContentGuid)
        {
            var testManager = _serviceLocator.GetInstance<ITestManager>();
            var aTest = testManager.GetTestByItemId(aContentGuid).Find(abTest => abTest.State != TestState.Done && abTest.State != TestState.Archived);

            if (aTest == null)
                aTest = new ABTest();

            return aTest;
        }

        public IMarketingTest GetTestById(Guid testGuid)
        {
            var testManager = _serviceLocator.GetInstance<ITestManager>();
            return testManager.Get(testGuid);
        }

        public void DeleteTestForContent(Guid aContentGuid)
        {
            var testManager = _serviceLocator.GetInstance<ITestManager>();
            var testList = testManager.GetTestByItemId(aContentGuid).FindAll(abtest => abtest.State != TestState.Done && abtest.State != TestState.Archived);

            foreach(var test in testList)
            {
                testManager.Delete(test.Id);
            }
        }

        /// <summary>
        /// For unit testing
        /// </summary>
        /// <param name="locator"></param>
        internal MarketingTestingWebRepository(IServiceLocator locator)
        {
            _serviceLocator = locator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public Guid CreateMarketingTest(TestingStoreModel testData)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();

            IMarketingTest test = ConvertToMarketingTest(testData);

            

            return tm.Save(test);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void DeleteMarketingTest(Guid testGuid)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();
            tm.Delete(testGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void StartMarketingTest(Guid testGuid)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();
            tm.Start(testGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void StopMarketingTest(Guid testGuid)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();
            tm.Stop(testGuid);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void ArchiveMarketingTest(Guid testObjectId, Guid winningVariantId, int version)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();
            tm.Archive(testObjectId, winningVariantId);
        }

        public Guid SaveMarketingTest(IMarketingTest testData)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();

            return tm.Save(testData);
        }
       
        public IMarketingTest ConvertToMarketingTest(TestingStoreModel testData)
        {
            IMarketingTest test = new ABTest();

            if (testData.StartDate == null)
            {
                testData.StartDate = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            }

            var content = _serviceLocator.GetInstance<IContentRepository>()
                .Get<IContent>( new ContentReference(testData.ConversionPage) );

            var kpi = new ContentComparatorKPI(content.ContentGuid)
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            test = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = testData.TestContentId,
                Owner = GetCurrentUser(),
                Description = testData.TestDescription,
                Title = testData.TestTitle,
                StartDate = DateTime.Parse(testData.StartDate).ToUniversalTime(),
                EndDate = CalculateEndDateFromDuration(testData.StartDate, testData.TestDuration),
                ParticipationPercentage = testData.ParticipationPercent,
                Variants = new List<Variant>
                {
                    new Variant() {Id=Guid.NewGuid(),ItemId = testData.TestContentId,ItemVersion = testData.PublishedVersion, Views = 0, Conversions = 0},
                    new Variant() {Id=Guid.NewGuid(),ItemId = testData.TestContentId,ItemVersion = testData.VariantVersion, Views = 0, Conversions = 0}
                },
                KpiInstances = new List<IKpi> { kpi },

            };

            if (DateTime.Now >= DateTime.Parse(testData.StartDate))
            {
                test.State = TestState.Active;
            }

            return test;
        }

        private string GetCurrentUser()
        {
            return PrincipalInfo.CurrentPrincipal.Identity.Name;
        }

        private DateTime? CalculateEndDateFromDuration(string startDate, int testDuration)
        {
            DateTime endDate = DateTime.Parse(startDate).ToUniversalTime();
            return endDate.AddDays(testDuration);
        }


    }
}
