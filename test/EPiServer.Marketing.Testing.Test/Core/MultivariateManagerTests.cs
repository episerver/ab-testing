using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Marketing.KPI.Dal.Model;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Dal.DataAccess;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;
using EPiServer.Core;
using EPiServer.Marketing.KPI.DataAccess;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.Testing.Core.Exceptions;
using EPiServer.Marketing.Testing.Core.Statistics;
using System.Runtime.Caching;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;

namespace EPiServer.Marketing.Testing.Test.Core
{
    public class MultivariateManagerTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<ITestingDataAccess> _dataAccessLayer;
        private Mock<IContentLoader> _contentLoader;
        private Guid testId = Guid.NewGuid();
        private Mock<IKpiManager> _kpiManager;
        private Mock<IKpiDataAccess> _kpiDataAccess;
        private Mock<DefaultMarketingTestingEvents> _marketingEvents;

        private TestManager GetUnitUnderTest()
        {
            // make sure the global mem cache that test manager uses is clean.
            MemoryCache.Default.Remove(TestManager.TestingCacheName);

            var dalList = new List<IABTest>()
            {
                new DalABTest() {
                    Id = testId,
                    EndDate = DateTime.Now.AddDays(1),
                    OriginalItemId = testId,
                    ConfidenceLevel = 95,
                    State = DalTestState.Active,
                    Variants = new List<DalVariant>()
                    {
                        new DalVariant() {ItemVersion = 1, Views = 5000, Conversions = 100}, new DalVariant()
                        {
                            ItemVersion = 4, Views = 5000, Conversions = 130, IsPublished=true,
                            DalKeyValueResults = new List<DalKeyValueResult>()
                            {
                                new DalKeyValueResult()
                                {
                                    KpiId = Guid.NewGuid(), Value = 12, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow, Id = Guid.NewGuid()
                                }
                            },
                            DalKeyFinancialResults = new List<DalKeyFinancialResult>()
                            {
                                new DalKeyFinancialResult() { KpiId = Guid.NewGuid(), Total = 12, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow, Id = Guid.NewGuid()}
                            }
                        }
                    },
                    KeyPerformanceIndicators = new List<DalKeyPerformanceIndicator>() {new DalKeyPerformanceIndicator()} }
            };

            _kpiDataAccess = new Mock<IKpiDataAccess>();
            _kpiDataAccess.Setup(call => call.Get(It.IsAny<Guid>())).Returns(new DalKpi());
            var kpi = new Kpi();
            _kpiManager = new Mock<IKpiManager>();
            _kpiManager.Setup(call => call.Get(It.IsAny<Guid>())).Returns(kpi);

            var startTest = new DalABTest() {Id = testId, State = DalTestState.Active, Variants = new List<DalVariant>(), KeyPerformanceIndicators = new List<DalKeyPerformanceIndicator>() };
            _serviceLocator = new Mock<IServiceLocator>();
            _dataAccessLayer = new Mock<ITestingDataAccess>();
            _dataAccessLayer.Setup(dal => dal.Get(It.IsAny<Guid>())).Returns(GetDalTest());
            _dataAccessLayer.Setup(dal => dal.Start(It.IsAny<Guid>())).Returns(startTest);
            _dataAccessLayer.Setup(dal => dal.GetTestList(It.IsAny<DalTestCriteria>())).Returns(dalList);
            _serviceLocator.Setup(sl => sl.GetInstance<ITestingDataAccess>()).Returns(_dataAccessLayer.Object);
            _serviceLocator.Setup(sl => sl.GetInstance<IKpiDataAccess>()).Returns(_kpiDataAccess.Object);
            _serviceLocator.Setup(sl => sl.GetInstance<IKpiManager>()).Returns(_kpiManager.Object);

            var pageRef2 = new PageReference() { ID = 2, WorkID = 0 };
            var contentData = new PageData(pageRef2);

            contentData.Property.Add(new PropertyNumber() { Name = "PageWorkStatus" });
            contentData.Property.Add(new PropertyDate() { Name = "PageStartPublish" });

            _contentLoader = new Mock<IContentLoader>();
            _contentLoader.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(new PageData());
            _contentLoader.Setup(call => call.Get<ContentData>(It.IsAny<ContentReference>())).Returns(contentData);
            _serviceLocator.Setup(sl => sl.GetInstance<IContentLoader>()).Returns(_contentLoader.Object);

            _marketingEvents = new Mock<DefaultMarketingTestingEvents>();
            _serviceLocator.Setup(sl => sl.GetInstance<DefaultMarketingTestingEvents>()).Returns(_marketingEvents.Object);

            return new TestManager(_serviceLocator.Object);
        }

        private DalABTest GetDalTest()
        {
            return new DalABTest
            {
                Variants = new List<DalVariant>
                {
                    new DalVariant {Id = Guid.NewGuid(), ItemId = Guid.NewGuid(), ItemVersion = 1}
                },
                KeyPerformanceIndicators = new List<DalKeyPerformanceIndicator>()
            };
        }

        private ABTest GetManagerTest()
        {
            return new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                Variants = new List<Variant>(),
                KpiInstances = new List<IKpi>(),
            };
        }

        [Fact]
        public void TestManager_CallsDataAccessGetWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();


            tm.Get(theGuid);

            _dataAccessLayer.Verify(da => da.Get(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer get was never called or Guid did not match.");
        }

        [Fact]
        public void TestManager_CallsDataAccessGetTestByItemId()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            var dalList = new List<IABTest>();

            var test = new DalABTest
            {
                OriginalItemId = theGuid,
                Variants = new List<DalVariant>(),
                KeyPerformanceIndicators = new List<DalKeyPerformanceIndicator>()
            };

            dalList.Add(test);
            _dataAccessLayer.Setup(dal => dal.GetTestByItemId(It.IsAny<Guid>())).Returns(dalList);
            _dataAccessLayer.Setup(dal => dal.GetTestList(It.IsAny<DalTestCriteria>())).Returns(dalList);
            var returnList = tm.GetActiveTestsByOriginalItemId(theGuid);

            Assert.True(returnList.All(t => t.OriginalItemId == theGuid),
                "DataAcessLayer GetTestByItemId was never called or Guid did not match.");
        }

        [Fact]
        public void TestManager_CallsGetTestListWithCritera()
        {
            var critera = new TestCriteria();
            var testFilter = new ABTestFilter
            {
                Operator = FilterOperator.And,
                Property = ABTestProperty.OriginalItemId,
                Value = "Test"
            };
            critera.AddFilter(testFilter);
            var tm = GetUnitUnderTest();
            var dalList = new List<IABTest>();
            dalList.Add(GetDalTest());
            _dataAccessLayer.Setup(dal => dal.GetTestList(It.IsAny<DalTestCriteria>())).Returns(dalList);
            tm.GetTestList(critera);

            _dataAccessLayer.Verify(
                da =>
                    da.GetTestList(
                        It.Is<DalTestCriteria>(arg => arg.GetFilters().First().Operator == DalFilterOperator.And &&
                                                      arg.GetFilters().First().Property ==
                                                      DalABTestProperty.OriginalItemId &&
                                                      arg.GetFilters().First().Value == testFilter.Value)),
                "DataAcessLayer GetTestList was never called or criteria did not match.");
        }

        [Fact]
        public void TestManager_CallsDeleteWithGuid()
        {
            //var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.Delete(testId);

            _dataAccessLayer.Verify(da => da.Delete(It.Is<Guid>(arg => arg.Equals(testId))),
                "DataAcessLayer Delete was never called or Guid did not match.");
        }

        [Fact]
        public void TestManager_CallsStartWithGuid()
        {
            //var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.Start(testId);

            _dataAccessLayer.Verify(da => da.Start(It.Is<Guid>(arg => arg.Equals(testId))),
                "DataAcessLayer Start was never called or Guid did not match.");
        }

        [Fact]
        public void TestManager_CallsStopWithGuid()
        {
            //var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.Stop(testId);

            _dataAccessLayer.Verify(da => da.Stop(It.Is<Guid>(arg => arg.Equals(testId))),
                "DataAcessLayer Stop was never called or Guid did not match.");
        }

        [Fact]
        public void TestManager_CallsArchiveWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var theVariantGuid = new Guid("e5187661-71a9-4209-8a5c-1f52f4be245b");
            var tm = GetUnitUnderTest();
            tm.Archive(theGuid,theVariantGuid);
            _dataAccessLayer.Verify(da => da.Archive(It.Is<Guid>(arg => arg.Equals(theGuid)),It.Is<Guid>(arg=>arg.Equals(theVariantGuid))),"DataAcessLayer Archive was never called or Guid did not match.");
        }

        [Fact]
        public void TestManager_CallsDataAccessLayerSaveWithProperArguments()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            var test = new ABTest
            {
                Id = theGuid,
                ModifiedDate = DateTime.UtcNow,
                Variants =
                    new List<Variant>
                    {
                        new Variant
                        {
                            Id = Guid.NewGuid(),
                            ItemId = Guid.NewGuid(),
                            ItemVersion = 1,
                            Views = 0,
                            Conversions = 0,
                            KeyFinancialResults = new List<KeyFinancialResult>(),
                            KeyValueResults = new List<KeyValueResult>()
                        }
                    },
                KpiInstances =
                    new List<IKpi>
                    {
                        new Kpi {Id = Guid.NewGuid(), CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow}
                    },
            };

            tm.Save(test);

            _dataAccessLayer.Verify(da => da.Save(It.Is<DalABTest>(arg => arg.Id == theGuid)),
                "DataAcessLayer Save was never called or object did not match.");
        }

        [Fact]
        public void TestManager_CallsIncrementCountWithProperArguments()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var theTestItemGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A4");
            var theItemVersion = 1;
            CountType type = CountType.Conversion;

            var tm = GetUnitUnderTest();
            tm.IncrementCount(theGuid, theTestItemGuid, theItemVersion, type);

            _dataAccessLayer.Verify(
                da =>
                    da.IncrementCount(It.Is<Guid>(arg => arg.Equals(theGuid)), It.IsAny<Guid>(), It.IsAny<int>(),
                        It.IsAny<DalCountType>()),
                "DataAcessLayer IncrementCount was never called or Test Guid did not match.");
            _dataAccessLayer.Verify(
                da =>
                    da.IncrementCount(It.IsAny<Guid>(), It.Is<Guid>(arg => arg.Equals(theTestItemGuid)), It.IsAny<int>(),
                        It.IsAny<DalCountType>()),
                "DataAcessLayer IncrementCount was never called or test item Guid did not match.");
            _dataAccessLayer.Verify(
                da =>
                    da.IncrementCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(),
                        It.Is<DalCountType>(arg => arg.Equals(DalCountType.Conversion))),
                "DataAcessLayer IncrementCount was never called or CountType did not match.");
        }

        [Fact]
        public void TestManager_CallsAddKpiResultWithProperArguments()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var theTestItemGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A4");
            var theItemVersion = 1;
            var resultsType = KeyResultType.Value;
            var result = new KeyValueResult()
            {
                KpiId = Guid.NewGuid(),
                Value = 12,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Id = Guid.NewGuid()
            };

            var result2 = new KeyFinancialResult()
            {
                KpiId = Guid.NewGuid(),
                Total = 12,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Id = Guid.NewGuid()
            };

            var tm = GetUnitUnderTest();
            tm.AddKpiResultData(theGuid, theTestItemGuid, theItemVersion, result, resultsType);

            _dataAccessLayer.Verify(
                da =>
                    da.AddKpiResultData(It.Is<Guid>(arg => arg.Equals(theGuid)), It.IsAny<Guid>(), It.IsAny<int>(),
                        It.IsAny<IDalKeyResult>(), It.IsAny<int>()),
                "DataAcessLayer AddKpiResultData was never called or Test Guid did not match.");
            _dataAccessLayer.Verify(
                da =>
                    da.AddKpiResultData(It.IsAny<Guid>(), It.Is<Guid>(arg => arg.Equals(theTestItemGuid)), It.IsAny<int>(),
                        It.IsAny<IDalKeyResult>(), It.IsAny<int>()),
                "DataAcessLayer AddKpiResultData was never called or test item Guid did not match.");
            _dataAccessLayer.Verify(
                da =>
                    da.AddKpiResultData(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(),
                        It.IsAny<IDalKeyResult>(), It.Is<int>(arg => arg.Equals((int)resultsType))),
                "DataAcessLayer AddKpiResultData was never called or CountType did not match.");

            tm.AddKpiResultData(theGuid, theTestItemGuid, theItemVersion, result2, 0);

            _dataAccessLayer.Verify(
                da =>
                    da.AddKpiResultData(It.Is<Guid>(arg => arg.Equals(theGuid)), It.IsAny<Guid>(), It.IsAny<int>(),
                        It.IsAny<IDalKeyResult>(), It.IsAny<int>()),
                "DataAcessLayer AddKpiResultData was never called or Test Guid did not match.");
            _dataAccessLayer.Verify(
                da =>
                    da.AddKpiResultData(It.IsAny<Guid>(), It.Is<Guid>(arg => arg.Equals(theTestItemGuid)), It.IsAny<int>(),
                        It.IsAny<IDalKeyResult>(), It.IsAny<int>()),
                "DataAcessLayer AddKpiResultData was never called or test item Guid did not match.");
            _dataAccessLayer.Verify(
                da =>
                    da.AddKpiResultData(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(),
                        It.IsAny<IDalKeyResult>(), It.Is<int>(arg => arg.Equals((int)resultsType))),
                "DataAcessLayer AddKpiResultData was never called or CountType did not match.");
        }

        [Fact]
        public void TestManager_ReturnLandingPage_NoCache()
        {
            // Make sure that the return landing page, calls data access layer if its not in the cache..
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var originalItemId = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A4");
            var vID = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A5");
            var variantList = new List<DalVariant> {new DalVariant {Id = vID}, new DalVariant {Id = originalItemId}};

            var tm = GetUnitUnderTest();
            _dataAccessLayer.Setup(da => da.Get(It.Is<Guid>(arg => arg.Equals(theGuid)))).Returns(
                new DalABTest
                {
                    Id = theGuid,
                    OriginalItemId = originalItemId,
                    Variants = variantList,
                    ParticipationPercentage = 100,
                    KeyPerformanceIndicators = new List<DalKeyPerformanceIndicator>()
                });

            var count = 0;
            var originalCalled = false;
            var variantCalled = false;
            // loop over call until all possible switch options are generated.
            while (count < 2)
            {
                // clear the cache if you have to (tm.clearCache() ?) - this test is supposed to verify that the 
                // database layer is called.
                var landingPage = tm.ReturnLandingPage(theGuid);

                if (landingPage.Id == originalItemId && !originalCalled)
                {
                    count++;
                    originalCalled = true;
                }

                if (landingPage.Id == vID && !variantCalled)
                {
                    count++;
                    variantCalled = true;
                }

                _dataAccessLayer.Verify(da => da.Get(It.Is<Guid>(arg => arg.Equals(theGuid))),
                    "DataAcessLayer get was never called or Guid did not match.");
                Assert.True(landingPage.Id.Equals(originalItemId) ||
                            landingPage.Id.Equals(vID), "landingPage is not the original quid or the variant quid");
            }
        }

        [Fact]
        public void TestManager_EmitUpdateConversion()
        {
            var testManager = GetUnitUnderTest();

            // Mock up the message manager
            Mock<IMessagingManager> messageManager = new Mock<IMessagingManager>();
            _serviceLocator.Setup(sl => sl.GetInstance<IMessagingManager>()).Returns(messageManager.Object);

            Guid original = Guid.NewGuid();
            Guid testItemId = Guid.NewGuid();
            testManager.EmitUpdateCount(original, testItemId, 1, CountType.Conversion);

            messageManager.Verify(mm => mm.EmitUpdateConversion(
                It.Is<Guid>(arg => arg.Equals(original)),
                It.Is<Guid>(arg => arg.Equals(testItemId)),
                It.Is<int>(arg => arg.Equals(1))),
                "Guids are not correct or update conversion message not emmited");
        }

        [Fact]
        public void TestManager_EmitUpdateView()
        {
            var testManager = GetUnitUnderTest();

            // Mock up the message manager
            Mock<IMessagingManager> messageManager = new Mock<IMessagingManager>();
            _serviceLocator.Setup(sl => sl.GetInstance<IMessagingManager>()).Returns(messageManager.Object);

            Guid original = Guid.NewGuid();
            Guid testItemId = Guid.NewGuid();
            testManager.EmitUpdateCount(original, testItemId, 1, CountType.View);

            messageManager.Verify(mm => mm.EmitUpdateViews(
                It.Is<Guid>(arg => arg.Equals(original)),
                It.Is<Guid>(arg => arg.Equals(testItemId)),
                It.Is<int>(arg => arg.Equals(1))),
                "Guids are not correct or update View message not emmited");
        }

        [Fact]
        public void TestManager_EmitKpiResultData()
        {
            var testManager = GetUnitUnderTest();

            // Mock up the message manager
            Mock<IMessagingManager> messageManager = new Mock<IMessagingManager>();
            _serviceLocator.Setup(sl => sl.GetInstance<IMessagingManager>()).Returns(messageManager.Object);

            var original = Guid.NewGuid();
            var testItemId = Guid.NewGuid();
            var result = new KeyFinancialResult()
            {
                KpiId = Guid.NewGuid(),
                Total = 12,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Id = Guid.NewGuid()
            };

            testManager.EmitKpiResultData(original, testItemId, 1, result, 0);

            messageManager.Verify(mm => mm.EmitKpiResultData(
                It.Is<Guid>(arg => arg.Equals(original)),
                It.Is<Guid>(arg => arg.Equals(testItemId)),
                It.Is<int>(arg => arg.Equals(1)),
                It.Is<IKeyResult>(arg => arg.Equals(result)),
                It.Is<KeyResultType>(arg => arg.Equals(KeyResultType.Financial))),
                "Guids are not correct or kpi result message not emmited");
        }

        [Fact]
        public void TestManager_EvaluateKpisReturnsEmptyIDList()
        {
            var testManager = GetUnitUnderTest();
            IList<IKpi> kpis = new List<IKpi>();
            Mock<IContent> content = new Mock<IContent>();
            IContent c = content.Object;
            c.ContentGuid = Guid.NewGuid();
            var retList = testManager.EvaluateKPIs(kpis, new ContentEventArgs(new ContentReference()) { Content = c } );
            Assert.True(retList != null, "EvaluateKPI method returned a null list, shouldnt do that");
            Assert.True(retList.Count() == 0, "EvaluateKPI method returned a list but it was not empty");
        }

        [Fact]
        public void TestManager_EvaluateKpisReturnsOneIDInList()
        {
            var testManager = GetUnitUnderTest();

            IList<IKpi> kpis = new List<IKpi>()
            {
                new TestKpi(Guid.NewGuid())
            };

            Mock<IContent> content = new Mock<IContent>();
            IContent c = content.Object;
            c.ContentGuid = Guid.NewGuid();
            var retList = testManager.EvaluateKPIs(kpis, new ContentEventArgs(new ContentReference()) { Content = c } );
            Assert.True(retList != null, "EvaluateKPI method returned a null list, shouldnt do that");
            Assert.True(retList.Count() == 1, "EvaluateKPI method returned a list that did not have one item in it");
        }

        [Fact]
        public void TestManager_UpdateCache()
        {
            var testManager = GetUnitUnderTest();
            var test = GetManagerTest();
            testManager.UpdateCache(test, CacheOperator.Add);

            Assert.Equal(2, testManager.ActiveCachedTests.Count());

            testManager.UpdateCache(test, CacheOperator.Remove);
            Assert.Equal(1, testManager.ActiveCachedTests.Count());
        }

        [Fact]
        public void CalculateSignificance()
        {
            var test = new ABTest()
            {
                ConfidenceLevel = 95,
                Variants =
                    new List<Variant>()
                    {
                        new Variant() {Views = 5000, Conversions = 130},
                        new Variant() {Views = 5000, Conversions = 100}
                    }
            };

            var results = Significance.CalculateIsSignificant(test);

            Assert.InRange(results.ZScore, 2.00, 2.01);
            Assert.True(results.IsSignificant);
        }

        [Fact]
        public void TestManager_Save_Throws_Null_KPIs()
        {
            var testManager = GetUnitUnderTest();
            var dalList = new List<IABTest>();
            _dataAccessLayer.Setup(dal => dal.GetTestList(It.IsAny<DalTestCriteria>())).Returns(dalList);

            var test = new ABTest
            {
                Id = Guid.NewGuid(),
                Variants = new List<Variant>()
            };

            try
            {
                testManager.Save(test);
            }
            catch (Exception e)
            {
                Assert.IsType<SaveTestException>(e);
            }
        }

        [Fact]
        public void TestManager_Save_Throws_Empty_KPIs()
        {
            var testManager = GetUnitUnderTest();
            var dalList = new List<IABTest>();
            _dataAccessLayer.Setup(dal => dal.GetTestList(It.IsAny<DalTestCriteria>())).Returns(dalList);

            var test = new ABTest
            {
                Id = Guid.NewGuid(),
                Variants = new List<Variant>(),
                KpiInstances = new List<IKpi>()
            };

            try
            {
                testManager.Save(test);
            }
            catch (Exception e)
            {
                Assert.IsType<SaveTestException>(e);
            }
        }

        [Fact]
        public void SaveTestExceptions()
        {
            var e = new SaveTestException("test", new Exception());

            Assert.IsType<SaveTestException>(new SaveTestException());
            Assert.Equal("test", e.Message);
        }
        [Fact]
        public void GetVariantPageData_Test()
        {
            var testManager = GetUnitUnderTest();

            var pageData = testManager.GetVariantContent(testId);

            Assert.NotNull(pageData);
        }
    }

    class TestKpi : Kpi
    {
        Guid _g;
        public TestKpi(Guid g) { _g = g; }

        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            if (_g == Guid.Empty)
                return new KpiConversionResult() {HasConverted = true};
            else
                return new KpiConversionResult() { HasConverted = false };
        }
    }
}