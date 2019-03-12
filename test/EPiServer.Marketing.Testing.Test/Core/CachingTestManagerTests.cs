using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.Marketing.Testing.Test
{
    public class CachingTestManagerTests
    {
        private Mock<ICacheSignal> _mockSignal;
        private Mock<DefaultMarketingTestingEvents> _mockEvents;
        private Mock<ITestManager> _mockTestManager;
        private List<IMarketingTest> _expectedTests;

        public CachingTestManagerTests()
        {
            _mockSignal = new Mock<ICacheSignal>();
            _mockEvents = new Mock<DefaultMarketingTestingEvents>();
            _mockTestManager = new Mock<ITestManager>();


            _expectedTests = new List<IMarketingTest>
            {
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-GB", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "es-ES", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } }
            };
        }

        [Fact]
        public void CachingTestManager_Ctor_PreparesCaching()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Ctor_PreparesCaching));
            
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            _mockTestManager.VerifyAll();
            _mockSignal.Verify(s => s.Set(), Times.Once());
            _mockSignal.Verify(s => s.Monitor(It.Is<Action>(a => a.Method.Name == "RefreshCache")), Times.Once());
            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestAddedToCacheEvent, It.IsAny<TestEventArgs>()), Times.Exactly(3));
        }

        [Fact]
        public void CachingTestManager_Archive_SignalsCacheInvalidation()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Archive_SignalsCacheInvalidation));

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var testToArchive = _expectedTests.First();
            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            manager.Archive(testToArchive.Id, testToArchive.Variants.First().Id);

            _mockTestManager.Verify(
                tm =>
                    tm.Archive(
                        It.Is<Guid>(actualTestId => actualTestId == testToArchive.Id),
                        It.Is<Guid>(actualVariantId => actualVariantId == testToArchive.Variants.First().Id),
                        null
                    ),
                Times.Once
            );

            _mockSignal.Verify(s => s.Reset(), Times.Exactly(_expectedTests.Count() + 1));
            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, It.IsAny<TestEventArgs>()), Times.Once);
        }

        [Fact]
        public void CachingTestManager_Delete_SignalsCacheInvalidation()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Delete_SignalsCacheInvalidation));

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var testToArchive = _expectedTests.First();
            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            manager.Delete(testToArchive.Id, new CultureInfo(testToArchive.ContentLanguage));

            _mockTestManager.Verify(
                tm =>
                    tm.Delete(
                        It.Is<Guid>(actualTestId => actualTestId == testToArchive.Id),
                        It.Is<CultureInfo>(actualContentCulture => actualContentCulture.Name == testToArchive.ContentLanguage)
                    ),
                Times.Once
            );

            _mockSignal.Verify(s => s.Reset(), Times.Exactly(_expectedTests.Count() + 1));
            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, It.IsAny<TestEventArgs>()), Times.Once);
        }

        [Fact]
        public void CachingTestManager_EvaluateKPIs_InvokesInnerManager()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_EvaluateKPIs_InvokesInnerManager));
            var expectedKpis = new List<IKpi>();
            var expectedResults = new List<IKpiResult>();
            var expectedSender = new object();
            var expectedEventArgs = new EventArgs();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(
                tm =>
                    tm.EvaluateKPIs(expectedKpis, expectedSender, expectedEventArgs)
            ).Returns(expectedResults);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualResults = manager.EvaluateKPIs(expectedKpis, expectedSender, expectedEventArgs);

            _mockTestManager.VerifyAll();

            Assert.Equal(expectedResults, actualResults);
        }

        [Fact]
        public void CachingTestManager_Get_DeliversTestFromCache()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Delete_SignalsCacheInvalidation));

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var expectedTest = _expectedTests.Last();
            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTest = manager.Get(expectedTest.Id, true);

            Assert.Equal(expectedTest, actualTest);

            _mockTestManager.Verify(tm => tm.Get(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never());
        }

        [Fact]
        public void CachingTestManager_Get_DeliversTestFromInnerManagerOnCacheMiss()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Get_DeliversTestFromInnerManagerOnCacheMiss));

            var expectedTest = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                ContentLanguage = "en-GB",
                Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
            };

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests); 
            _mockTestManager.Setup(tm => tm.Get(expectedTest.Id, false)).Returns(expectedTest);
            
            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTest = manager.Get(expectedTest.Id, true);

            Assert.Equal(expectedTest, actualTest);

            _mockTestManager.VerifyAll();
        }

        [Fact]
        public void CachingTestManager_Get_SkipsCacheIfRequested()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Get_DeliversTestFromInnerManagerOnCacheMiss));

            var expectedTest = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                ContentLanguage = "en-GB",
                State = TestState.Active,
                Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
            };

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.Get(expectedTest.Id, false)).Returns(expectedTest);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTest = manager.Get(expectedTest.Id, false);

            Assert.Equal(expectedTest, actualTest);

            _mockTestManager.VerifyAll();
        }

        [Fact]
        public void CachingTestManager_GetsActiveTestsByOriginalItemId_DeliversActiveTest()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Get_DeliversTestFromInnerManagerOnCacheMiss));

            var expectedItem = _expectedTests.First();
            var expectedItemId = expectedItem.OriginalItemId;

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTests = manager.GetActiveTestsByOriginalItemId(expectedItemId);

            Assert.True(actualTests.Count == 1);
            Assert.Equal(expectedItem, actualTests.First());
        }

        [Fact]
        public void CachingTestManager_GetActiveTestsByOriginalItemId_DeliversEmptyListWhenTestNotFound()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Get_DeliversTestFromInnerManagerOnCacheMiss));

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTests = manager.GetActiveTestsByOriginalItemId(Guid.NewGuid());

            Assert.True(actualTests.Count == 0);            
        }

        [Fact]
        public void CachingTestManager_GetsActiveTestsByOriginalItemId_WithCulture_DeliversActiveTest()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Get_DeliversTestFromInnerManagerOnCacheMiss));

            var expectedItem = _expectedTests.First();
            var expectedItemId = expectedItem.OriginalItemId;
            var expectedContentLanguage = expectedItem.ContentLanguage;
            var expectedCulture = new CultureInfo(expectedContentLanguage);

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTests = manager.GetActiveTestsByOriginalItemId(expectedItemId, expectedCulture);

            Assert.True(actualTests.Count == 1);
            Assert.Equal(expectedItem, actualTests.First());
        }

        [Fact]
        public void CachingTestManager_GetActiveTestsByOriginalItemId_WithCulture_DeliversEmptyListWhenTestNotFound()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Get_DeliversTestFromInnerManagerOnCacheMiss));

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTests = manager.GetActiveTestsByOriginalItemId(
                _expectedTests.First().OriginalItemId, 
                new CultureInfo(_expectedTests.Last().ContentLanguage)
            );

            Assert.True(actualTests.Count == 0);
        }

        [Fact]
        public void CachingTestManager_GetDatabaseVersion_InvokesInnerManager()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_GetDatabaseVersion_InvokesInnerManager));

            var expectedDbConnection = Mock.Of<DbConnection>();
            var expectedSchema = "schema";
            var expectedContextKey = "conext-key";
            var expectedPopulateCache = true;            

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetDatabaseVersion(expectedDbConnection, expectedSchema, expectedContextKey, expectedPopulateCache)).Returns(100L);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualVersion = manager.GetDatabaseVersion(expectedDbConnection, expectedSchema, expectedContextKey, expectedPopulateCache);

            _mockTestManager.VerifyAll();

            Assert.Equal(100L, actualVersion);
        }

        [Fact]
        public void CachingTestManager_GetTestByItemId_InvokesInnerManager()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_GetTestByItemId_InvokesInnerManager));

            var expectedItemId = Guid.NewGuid();
            var expectedTests = new List<IMarketingTest>();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetTestByItemId(expectedItemId)).Returns(expectedTests);
            
            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTests = manager.GetTestByItemId(expectedItemId);

            _mockTestManager.VerifyAll();

            Assert.Equal(expectedTests, actualTests);
        }

        [Fact]
        public void CachingTestManager_GetTestList_InvokesInnerManager()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_GetTestList_InvokesInnerManager));

            var expectedCriteria = new TestCriteria();
            var expectedTests = new List<IMarketingTest>();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetTestList(expectedCriteria)).Returns(expectedTests);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTests = manager.GetTestList(expectedCriteria);

            _mockTestManager.VerifyAll();

            Assert.Equal(expectedTests, actualTests);
        }

        [Fact]
        public void CachingTestManager_GetVariantContent_WithCulture_DeliversFromCacheAfterFirstRetrieval()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_GetVariantContent_WithCulture_DeliversFromCacheAfterFirstRetrieval));

            var expectedTest = _expectedTests.First();
            var expectedItemId = expectedTest.OriginalItemId;
            var expectedContentLanguage = expectedTest.ContentLanguage;
            var expectedCulture = new CultureInfo(expectedContentLanguage);
            var expectedVariant = Mock.Of<IContent>();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetVariantContent(expectedItemId, expectedCulture)).Returns(expectedVariant);
            
            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualVariant = manager.GetVariantContent(expectedItemId, expectedCulture);

            _mockTestManager.Verify(tm => tm.GetVariantContent(expectedItemId, expectedCulture), Times.Once());

            Assert.Equal(expectedVariant, actualVariant);

            var actualVariantFromCache = manager.GetVariantContent(expectedItemId, expectedCulture);

            _mockTestManager.Verify(tm => tm.GetVariantContent(expectedItemId, expectedCulture), Times.Once());

            Assert.Equal(expectedVariant, actualVariantFromCache);
        }

        [Fact]
        public void CachingTestManager_GetVariantContent_DoesNotAddNullVariantToCache()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_GetVariantContent_DoesNotAddNullVariantToCache));

            var expectedTest = _expectedTests.First();
            var expectedItemId = expectedTest.OriginalItemId;
            var expectedContentLanguage = expectedTest.ContentLanguage;
            var expectedCulture = new CultureInfo(expectedContentLanguage);
            var expectedVariant = Mock.Of<IContent>();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetVariantContent(expectedItemId, expectedCulture)).Returns((IContent)null);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            var expectedCacheItemCount = cache.Count();

            var actualVariant = manager.GetVariantContent(expectedItemId, expectedCulture);

            var actualCacheItemCount = cache.Count();

            Assert.Null(actualVariant);
            Assert.Equal(expectedCacheItemCount, actualCacheItemCount);
        }

        [Fact]
        public void CachingTestManager_GetVariantContent_DoesNotFindVariantIfAssociatedTestIsRemoved()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_GetVariantContent_DoesNotFindVariantIfAssociatedTestIsRemoved));

            var expectedTest = _expectedTests.First();
            var expectedItemId = expectedTest.OriginalItemId;
            var expectedContentLanguage = expectedTest.ContentLanguage;
            var expectedCulture = new CultureInfo(expectedContentLanguage);
            var expectedVariant = Mock.Of<IContent>();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.SetupSequence(tm => tm.GetVariantContent(expectedItemId, expectedCulture))
                            .Returns(expectedVariant)
                            .Returns(null);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            // Prime cache by invoking GetVariantContent()
            var actualVariant = manager.GetVariantContent(expectedItemId, expectedCulture);

            _mockTestManager.Verify(tm => tm.GetVariantContent(expectedItemId, expectedCulture), Times.Once());

            Assert.Equal(expectedVariant, actualVariant);

            // Ensure that variants are now being delivered from cache by invoking again

            actualVariant = manager.GetVariantContent(expectedItemId, expectedCulture);

            _mockTestManager.Verify(tm => tm.GetVariantContent(expectedItemId, expectedCulture), Times.Once());

            Assert.Equal(expectedVariant, actualVariant);

            // Delete associated test and verify that manager cannot find variant in the 
            // cache (by ensuring that it defered to the inner manager)

            manager.Delete(expectedTest.Id);

            actualVariant = manager.GetVariantContent(expectedItemId, expectedCulture);

            _mockTestManager.Verify(tm => tm.GetVariantContent(expectedItemId, expectedCulture), Times.Exactly(2));

            Assert.Null(actualVariant);
        }

        [Fact]
        public void CachingTestManager_IncrementCount_WithCriteria_InvokesInnerManager()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_IncrementCount_WithCriteria_InvokesInnerManager));

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var expectedCriteria = new IncrementCountCriteria();            
            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            manager.IncrementCount(expectedCriteria);

            _mockTestManager.Verify(tm => tm.IncrementCount(expectedCriteria), Times.Once());
        }

        [Fact]
        public void CachingTestManager_IncrementCount_InvokesInnerManager()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_IncrementCount_InvokesInnerManager));

            var expectedTestId = Guid.NewGuid();
            var expectedItemVersion = 10;
            var expectedResultType = CountType.Conversion;
            var expectedKpiId = Guid.NewGuid();
            var expectedAsync = false;

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            manager.IncrementCount(expectedTestId, expectedItemVersion, expectedResultType, expectedKpiId, expectedAsync);

            _mockTestManager.Verify(tm => tm.IncrementCount(expectedTestId, expectedItemVersion, expectedResultType, expectedKpiId, expectedAsync), Times.Once());
        }

        [Fact]
        public void CachingTestManager_ReturnLandingPage_InvokesInnerManager()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_ReturnLandingPage_InvokesInnerManager));

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var expectedTestId = Guid.NewGuid();
            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            manager.ReturnLandingPage(expectedTestId);

            _mockTestManager.Verify(tm => tm.ReturnLandingPage(expectedTestId), Times.Once());
        }

        [Fact]
        public void CachingTestManager_Save_CachesTestIfActive()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Save_CachesTestIfActive));

            var expectedTest = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                ContentLanguage = "en-GB",
                State = TestState.Active,
                Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
            };

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.Save(expectedTest)).Returns(expectedTest.Id);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTestId = manager.Save(expectedTest);

            Assert.Equal(expectedTest.Id, actualTestId);

            _mockTestManager.VerifyAll();
            _mockEvents.Verify(
                e => 
                    e.RaiseMarketingTestingEvent(
                        DefaultMarketingTestingEvents.TestAddedToCacheEvent, 
                        It.Is<TestEventArgs>(args => args.Test == expectedTest)
                    ), 
                Times.Once
            );

            _mockSignal.Verify(s => s.Reset(), Times.Exactly(_expectedTests.Count() + 1));
        }

        [Fact]
        public void CachingTestManager_Save_DoesNotCacheTestIfNotActive()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Save_DoesNotCacheTestIfNotActive));

            var expectedTest = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                ContentLanguage = "en-GB",
                State = TestState.Inactive,
                Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
            };

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.Save(expectedTest)).Returns(expectedTest.Id);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTestId = manager.Save(expectedTest);

            Assert.Equal(expectedTest.Id, actualTestId);

            _mockTestManager.VerifyAll();
            _mockEvents.Verify(
                e =>
                    e.RaiseMarketingTestingEvent(
                        DefaultMarketingTestingEvents.TestAddedToCacheEvent,
                        It.Is<TestEventArgs>(args => args.Test == expectedTest)
                    ),
                Times.Never
            );

            _mockSignal.Verify(s => s.Reset(), Times.Exactly(_expectedTests.Count()));
        }

        [Fact]
        public void CachingTestManager_Start_CachesTestIfActive()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Start_CachesTestIfActive));

            var expectedTest = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                ContentLanguage = "en-GB",
                State = TestState.Active,
                Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
            };

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.Start(expectedTest.Id)).Returns(expectedTest);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTest = manager.Start(expectedTest.Id);

            Assert.Equal(expectedTest, actualTest);

            _mockTestManager.VerifyAll();
            _mockEvents.Verify(
                e =>
                    e.RaiseMarketingTestingEvent(
                        DefaultMarketingTestingEvents.TestAddedToCacheEvent,
                        It.Is<TestEventArgs>(args => args.Test == expectedTest)
                    ),
                Times.Once
            );

            _mockSignal.Verify(s => s.Reset(), Times.Exactly(_expectedTests.Count() + 1));
        }

        [Fact]
        public void CachingTestManager_Start_DoesNotCacheTestIfNotActive()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Start_DoesNotCacheTestIfNotActive));

            var expectedTest = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                ContentLanguage = "en-GB",
                State = TestState.Inactive,
                Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
            };

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.Start(expectedTest.Id)).Returns(expectedTest);

            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTest = manager.Start(expectedTest.Id);

            Assert.Equal(expectedTest, actualTest);

            _mockTestManager.VerifyAll();
            _mockEvents.Verify(
                e =>
                    e.RaiseMarketingTestingEvent(
                        DefaultMarketingTestingEvents.TestAddedToCacheEvent,
                        It.Is<TestEventArgs>(args => args.Test == expectedTest)
                    ),
                Times.Never
            );

            _mockSignal.Verify(s => s.Reset(), Times.Exactly(_expectedTests.Count()));
        }

        [Fact]
        public void CachingTestManager_Stop_SignalsCacheInvalidation()
        {
            var cache = new MemoryCache(nameof(CachingTestManager_Archive_SignalsCacheInvalidation));

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var expectedTest = _expectedTests.First();
            var manager = new CachingTestManager(cache, _mockSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            manager.Stop(expectedTest.Id);

            _mockTestManager.Verify(
                tm =>
                    tm.Stop(
                        It.Is<Guid>(actualTestId => actualTestId == expectedTest.Id),
                        null
                    ),
                Times.Once
            );

            _mockSignal.Verify(s => s.Reset(), Times.Exactly(_expectedTests.Count() + 1));
            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, It.IsAny<TestEventArgs>()), Times.Once);
        }
    }
}
