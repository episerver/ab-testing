using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Data;
using System.Diagnostics.CodeAnalysis;
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
        public void ArchiveMarketingTest(Guid testGuid)
        {
            ITestManager tm = _serviceLocator.GetInstance<ITestManager>();
            tm.Archive(testGuid);
        }

       
        public IMarketingTest ConvertToMarketingTest(TestingStoreModel testData)
        {
            IMarketingTest test = new ABTest();

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
                StartDate = DateTime.Parse(testData.StartDate).ToLocalTime(),
                EndDate = CalculateEndDateFromDuration(testData.StartDate, testData.TestDuration),
                ParticipationPercentage = testData.ParticipationPercent,
                Variants = new List<Variant>
                {
                    new Variant() {Id=Guid.NewGuid(),ItemId = testData.TestContentId,ItemVersion = testData.PublishedVersion},
                    new Variant() {Id=Guid.NewGuid(),ItemId = testData.TestContentId,ItemVersion = testData.VariantVersion}
                },
                KpiInstances = new List<IKpi> { kpi },

            };

            test.TestResults = GenerateTestResultList(test.Variants);

            return test;
        }

        private List<TestResult> GenerateTestResultList(List<Variant> variants)
        {
            return variants.Select(v => new TestResult()
            {
                Id = Guid.NewGuid(), ItemId = v.Id, ItemVersion = v.ItemVersion
            }).ToList();
        }

        private string GetCurrentUser()
        {
            return PrincipalInfo.CurrentPrincipal.Identity.Name;
        }

        private DateTime? CalculateEndDateFromDuration(string startDate, int testDuration)
        {
            DateTime endDate = DateTime.Parse(startDate).ToLocalTime();
            return endDate.AddDays(testDuration);
        }











       

    }
}
