using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Test.Asserts;
using EPiServer.Shell.Gadgets;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Core
{
    public class CachingTestManagerTests
    {
        private Mock<ICacheSignal> _mockRemoteCacheSignal;
        private Mock<ICacheSignal> _mockConfigurationSignal;
        private Mock<DefaultMarketingTestingEvents> _mockEvents;
        private Mock<ITestManager> _mockTestManager;
        private List<IMarketingTest> _expectedTests;
        private Mock<ISynchronizedObjectInstanceCache> _mockSynchronizedObjectInstanceCache; 

        public CachingTestManagerTests()
        {
            _mockRemoteCacheSignal = new Mock<ICacheSignal>();
            _mockConfigurationSignal = new Mock<ICacheSignal>();

            _mockEvents = new Mock<DefaultMarketingTestingEvents>();
            _mockTestManager = new Mock<ITestManager>();

            _mockSynchronizedObjectInstanceCache = new Mock<ISynchronizedObjectInstanceCache>();

            _expectedTests = new List<IMarketingTest>
            {
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-GB", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "en-US", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "es-ES", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } }
            };
        }

        [Fact]
        public void CachingTestManager_Ctor_PreparesCaching()
        {
            _mockTestManager.Setup(tm => tm.GetTestList(It.Is<TestCriteria>( tc => 
                tc.GetFilters().FirstOrDefault().Property == ABTestProperty.State &&
                tc.GetFilters().FirstOrDefault().Operator == FilterOperator.And &&
                tc.GetFilters().FirstOrDefault().Value.ToString()  == TestState.Active.ToString()
            ))).Returns(_expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            _mockTestManager.VerifyAll();
            _mockRemoteCacheSignal.Verify(s => s.Set(), Times.Once());
            _mockRemoteCacheSignal.Verify(s => s.Reset(), Times.Never());
            _mockRemoteCacheSignal.Verify(s => s.Monitor(It.Is<Action>(a => a.Method.Name == "RefreshCache")), Times.Once());
            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestAddedToCacheEvent, It.IsAny<TestEventArgs>()), Times.Exactly(_expectedTests.Count()));
            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, It.IsAny<TestEventArgs>()), Times.Never());

            _mockConfigurationSignal.Verify(s => s.Reset(), Times.Never());
        }

        [Fact]
        public void CachingTestManager_Archive_SignalsCacheInvalidation()
        {
            var testToArchive = _expectedTests.First();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(It.IsAny<string>())).Returns(testToArchive);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            _mockRemoteCacheSignal.ResetCalls();

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

            _mockRemoteCacheSignal.Verify(s => s.Reset(), Times.Once());
            _mockConfigurationSignal.Verify(s => s.Reset(), Times.Once());

            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, It.IsAny<TestEventArgs>()), Times.Once);
        }

        [Fact]
        public void CachingTestManager_Delete_SignalsCacheInvalidation()
        {
            var expectedTest = _expectedTests.First();
            var expectedCultureInfo = CultureInfo.GetCultureInfo(expectedTest.ContentLanguage);

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var expectedKey = CachingTestManager.GetCacheKeyForTest(expectedTest.Id);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(expectedKey)).Returns(expectedTest);

            _mockRemoteCacheSignal.ResetCalls();

            manager.Delete(expectedTest.Id, expectedCultureInfo);

            _mockTestManager.Verify( tm => tm.Delete(expectedTest.Id, expectedCultureInfo), Times.Once );

            _mockRemoteCacheSignal.Verify(s => s.Reset(), Times.Once());
            _mockConfigurationSignal.Verify(s => s.Reset(), Times.Once());
            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, It.IsAny<TestEventArgs>()), Times.Once);

            _mockSynchronizedObjectInstanceCache.VerifyAll();
        }

        [Fact]
        public void CachingTestManager_EvaluateKPIs_InvokesInnerManager()
        {
            var expectedKpis = new List<IKpi>();
            var expectedResults = new List<IKpiResult>();
            var expectedSender = new object();
            var expectedEventArgs = new EventArgs();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(
                tm =>
                    tm.EvaluateKPIs(expectedKpis, expectedSender, expectedEventArgs)
            ).Returns(expectedResults);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualResults = manager.EvaluateKPIs(expectedKpis, expectedSender, expectedEventArgs);

            _mockTestManager.VerifyAll();

            Assert.Equal(expectedResults, actualResults);
        }

        [Fact]
        public void CachingTestManager_Get_DeliversTestFromCache()
        {
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var expectedTest = _expectedTests.Last();
            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTest = manager.Get(expectedTest.Id, true);

            Assert.Equal(expectedTest, actualTest);

            _mockTestManager.Verify(tm => tm.Get(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never());
        }

        [Fact]
        public void CachingTestManager_Get_DeliversTestFromInnerManagerOnCacheMiss()
        {
            var expectedTest = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                ContentLanguage = "en-GB",
                Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
            };

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests); 
            _mockTestManager.Setup(tm => tm.Get(expectedTest.Id, false)).Returns(expectedTest);
            
            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTest = manager.Get(expectedTest.Id, true);

            Assert.Equal(expectedTest, actualTest);

            _mockTestManager.VerifyAll();
        }

        [Fact]
        public void CachingTestManager_Get_SkipsCacheIfRequested()
        {
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

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTest = manager.Get(expectedTest.Id, false);

            Assert.Equal(expectedTest, actualTest);

            _mockTestManager.VerifyAll();
        }

        [Fact]
        public void CachingTestManager_GetsActiveTestsByOriginalItemId_DeliversActiveTest()
        {
            var expectedItem = _expectedTests.First();
            var expectedItemId = expectedItem.OriginalItemId;

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTests = manager.GetActiveTestsByOriginalItemId(expectedItemId);

            Assert.True(actualTests.Count == 1);
            Assert.Equal(expectedItem, actualTests.First());
        }

        [Fact]
        public void CachingTestManager_GetActiveTestsByOriginalItemId_DeliversEmptyListWhenTestNotFound()
        {
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(CachingTestManager.AllTestsKey)).Returns(_expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTests = manager.GetActiveTestsByOriginalItemId(Guid.NewGuid());

            Assert.True(actualTests.Count == 0);
            _mockSynchronizedObjectInstanceCache.VerifyAll();
        }

        [Fact]
        public void CachingTestManager_GetsActiveTestsByOriginalItemId_WithCulture_DeliversActiveTest()
        {
            var expectedItem = _expectedTests.First();
            var expectedItemId = expectedItem.OriginalItemId;
            var expectedContentLanguage = expectedItem.ContentLanguage;
            var expectedCulture = CultureInfo.GetCultureInfo(expectedContentLanguage);

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTests = manager.GetActiveTestsByOriginalItemId(expectedItemId, expectedCulture);

            Assert.True(actualTests.Count == 1);
            Assert.Equal(expectedItem, actualTests.First());
        }

        [Fact]
        public void CachingTestManager_GetActiveTestsByOriginalItemId_WithCulture_DeliversEmptyListWhenTestNotFound()
        {
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(CachingTestManager.AllTestsKey)).Returns(_expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTests = manager.GetActiveTestsByOriginalItemId(
                _expectedTests.First().OriginalItemId, 
                CultureInfo.GetCultureInfo(_expectedTests.Last().ContentLanguage)
            );

            Assert.True(actualTests.Count == 0);
            _mockSynchronizedObjectInstanceCache.VerifyAll();
        }

        [Fact]
        public void CachingTestManager_GetDatabaseVersion_InvokesInnerManager()
        {
            var expectedDbConnection = Mock.Of<DbConnection>();
            var expectedSchema = "schema";
            var expectedContextKey = "conext-key";
            var expectedPopulateCache = true;            

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetDatabaseVersion(expectedDbConnection, expectedSchema, expectedContextKey, expectedPopulateCache)).Returns(100L);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualVersion = manager.GetDatabaseVersion(expectedDbConnection, expectedSchema, expectedContextKey, expectedPopulateCache);

            _mockTestManager.VerifyAll();

            Assert.Equal(100L, actualVersion);
        }

        [Fact]
        public void CachingTestManager_GetTestByItemId_InvokesInnerManager()
        {
            var expectedItemId = Guid.NewGuid();
            var expectedTests = new List<IMarketingTest>();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetTestByItemId(expectedItemId)).Returns(expectedTests);
            
            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTests = manager.GetTestByItemId(expectedItemId);

            _mockTestManager.VerifyAll();

            Assert.Equal(expectedTests, actualTests);
        }

        [Fact]
        public void CachingTestManager_GetTestList_InvokesInnerManager()
        {
            var expectedCriteria = new TestCriteria();
            var expectedTests = new List<IMarketingTest>();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetTestList(expectedCriteria)).Returns(expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualTests = manager.GetTestList(expectedCriteria);

            _mockTestManager.VerifyAll();

            Assert.Equal(expectedTests, actualTests);
        }

        [Fact]
        public void CachingTestManager_GetVariantContent_WithCulture_DeliversFromCacheAfterFirstRetrieval()
        {
            var expectedTest = _expectedTests.First();
            var expectedItemId = expectedTest.OriginalItemId;
            var expectedContentLanguage = expectedTest.ContentLanguage;
            var expectedCulture = CultureInfo.GetCultureInfo(expectedContentLanguage);
            var expectedVariant = Mock.Of<IContent>();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetVariantContent(expectedItemId, expectedCulture)).Returns(expectedVariant);
            
            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            var actualVariant = manager.GetVariantContent(expectedItemId, expectedCulture);

            _mockTestManager.Verify(tm => tm.GetVariantContent(expectedItemId, expectedCulture), Times.Once());

            Assert.Equal(expectedVariant, actualVariant);

            var actualVariantFromCache = manager.GetVariantContent(expectedItemId, expectedCulture);

            _mockTestManager.Verify(tm => tm.GetVariantContent(expectedItemId, expectedCulture), Times.Once());

            Assert.Equal(expectedVariant, actualVariantFromCache);
        }

        //[Fact]
        //public void CachingTestManager_GetVariantContent_DoesNotAddNullVariantToCache()
        //{
        //    var expectedTest = _expectedTests.First();
        //    var expectedItemId = expectedTest.OriginalItemId;
        //    var expectedContentLanguage = expectedTest.ContentLanguage;
        //    var expectedCulture = CultureInfo.GetCultureInfo(expectedContentLanguage);
        //    var expectedVariant = Mock.Of<IContent>();

        //    _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
        //    _mockTestManager.Setup(tm => tm.GetVariantContent(expectedItemId, expectedCulture)).Returns((IContent)null);

        //    var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

        //    var expectedCacheItemCount = cache.Count();

        //    var actualVariant = manager.GetVariantContent(expectedItemId, expectedCulture);

        //    var actualCacheItemCount = cache.Count();

        //    Assert.Null(actualVariant);
        //    Assert.Equal(expectedCacheItemCount, actualCacheItemCount);
        //}

        //[Fact]
        //public void CachingTestManager_GetVariantContent_DoesNotFindVariantIfAssociatedTestIsRemoved()
        //{
        //    var expectedTest = _expectedTests.First();
        //    var expectedItemId = expectedTest.OriginalItemId;
        //    var expectedContentLanguage = expectedTest.ContentLanguage;
        //    var expectedCulture = CultureInfo.GetCultureInfo(expectedContentLanguage);
        //    var expectedVariant = Mock.Of<IContent>();

        //    _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
        //    _mockTestManager.SetupSequence(tm => tm.GetVariantContent(expectedItemId, expectedCulture))
        //                    .Returns(expectedVariant)
        //                    .Returns(null);
        //    _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(CachingTestManager.GetCacheKeyForTest(expectedTest.Id))).Returns(expectedTest);

        //    var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

        //    // Delete associated test and verify that manager cannot find variant in the 
        //    // cache (by ensuring that it defered to the inner manager)
            
        //    manager.Delete(expectedTest.Id);

        //    var actualVariant = manager.GetVariantContent(expectedItemId, expectedCulture);

        //    _mockTestManager.Verify(tm => tm.GetVariantContent(expectedItemId, expectedCulture), Times.Once());
        //    _mockSynchronizedObjectInstanceCache.VerifyAll();

        //    Assert.Null(actualVariant);
        //}

        [Fact]
        public void CachingTestManager_IncrementCount_WithCriteria_InvokesInnerManager()
        {
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var expectedCriteria = new IncrementCountCriteria();            
            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            manager.IncrementCount(expectedCriteria);

            _mockTestManager.Verify(tm => tm.IncrementCount(expectedCriteria), Times.Once());
        }

        [Fact]
        public void CachingTestManager_IncrementCount_InvokesInnerManager()
        {
            var expectedTestId = Guid.NewGuid();
            var expectedItemVersion = 10;
            var expectedResultType = CountType.Conversion;
            var expectedKpiId = Guid.NewGuid();
            var expectedAsync = false;

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            manager.IncrementCount(expectedTestId, expectedItemVersion, expectedResultType, expectedKpiId, expectedAsync);

            _mockTestManager.Verify(tm => tm.IncrementCount(expectedTestId, expectedItemVersion, expectedResultType, expectedKpiId, expectedAsync), Times.Once());
        }

        [Fact]
        public void CachingTestManager_ReturnLandingPage_InvokesInnerManager()
        {
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var expectedTestId = Guid.NewGuid();
            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);
            manager.ReturnLandingPage(expectedTestId);

            _mockTestManager.Verify(tm => tm.ReturnLandingPage(expectedTestId), Times.Once());
        }

        [Fact]
        public void CachingTestManager_Save_CachesTestIfActive()
        {
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

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            _mockRemoteCacheSignal.ResetCalls();

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

            _mockRemoteCacheSignal.Verify(s => s.Reset(), Times.Once());
            _mockConfigurationSignal.Verify(s => s.Reset(), Times.Once());
        }

        [Fact]
        public void CachingTestManager_Save_RemovesFromCacheIfNotActive_SendsMessageToReset()
        {
            var expectedTest = _expectedTests.First();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.Save(expectedTest)).Returns(expectedTest.Id);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            _mockRemoteCacheSignal.ResetCalls();

            expectedTest.State = TestState.Inactive;
            var actualTestId = manager.Save(expectedTest);

            Assert.Equal(expectedTest.Id, actualTestId);

            _mockTestManager.VerifyAll();
            _mockEvents.Verify( e =>
                                e.RaiseMarketingTestingEvent(
                                DefaultMarketingTestingEvents.TestAddedToCacheEvent,
                                It.Is<TestEventArgs>(args => args.Test == expectedTest)
                                ),
                                Times.Once // Once During constructor
            );
            _mockEvents.Verify( e =>
                                e.RaiseMarketingTestingEvent(
                                    DefaultMarketingTestingEvents.TestRemovedFromCacheEvent,
                                    It.Is<TestEventArgs>(args => args.Test == expectedTest)
                                ),
                                Times.Once // once when removed
             );
            _mockRemoteCacheSignal.Verify(s => s.Reset(), Times.Once());
            _mockConfigurationSignal.Verify(s => s.Reset(), Times.Once());
        }

        [Fact]
        public void CachingTestManager_Start_CachesTestIfActive()
        {
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

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            _mockRemoteCacheSignal.ResetCalls();

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

            _mockRemoteCacheSignal.Verify(s => s.Reset(), Times.Once());
            _mockConfigurationSignal.Verify(s => s.Reset(), Times.Once());
        }

        [Fact]
        public void CachingTestManager_Start_DoesNotCacheTestIfNotActive()
        {
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

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            _mockRemoteCacheSignal.ResetCalls();

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

            _mockRemoteCacheSignal.Verify(s => s.Reset(), Times.Never());
            _mockConfigurationSignal.Verify(s => s.Reset(), Times.Never());
        }

        [Fact]
        public void CachingTestManager_Stop_SignalsCacheInvalidation()
        {
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var expectedTest = _expectedTests.First();
            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            _mockRemoteCacheSignal.ResetCalls();

            manager.Stop(expectedTest.Id);

            _mockTestManager.Verify(
                tm =>
                    tm.Stop(
                        It.Is<Guid>(actualTestId => actualTestId == expectedTest.Id),
                        null
                    ),
                Times.Once
            );

            _mockRemoteCacheSignal.Verify(s => s.Reset(), Times.Once());
            _mockConfigurationSignal.Verify(s => s.Reset(), Times.Once());

            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, It.IsAny<TestEventArgs>()), Times.Once);
        }

        [Fact]
        public void RefreshCache_BuildsCache()
        {
            var expectedTests = new List<IMarketingTest>()
            {
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "es-ES", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), ContentLanguage = "es-ES", State = TestState.Active, Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } }
            };

            var expectedTestCriteria = new TestCriteria();
            expectedTestCriteria.AddFilter(
                new ABTestFilter
                {
                    Property = ABTestProperty.State,
                    Operator = FilterOperator.And,
                    Value = TestState.Active
                }
            );

            _mockTestManager.Setup(tm => tm.GetTestList(It.Is<TestCriteria>(tc =>
                                                AssertTestCriteria.AreEquivalent(expectedTestCriteria, tc))))
                                                .Returns(expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(c => c.Remove(CachingTestManager.MasterCacheKey));
            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.AllTestsKey,
                                                                     expectedTests,
                                                                     It.Is<CacheEvictionPolicy>(actual => 
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(null, 
                                                                            new string[] { CachingTestManager.MasterCacheKey }), actual))));
            _mockRemoteCacheSignal.Setup(c => c.Set()).Verifiable();

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, 
                                                 _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            _mockRemoteCacheSignal.ResetCalls();

            manager.RefreshCache();

            _mockTestManager.VerifyAll();
            _mockRemoteCacheSignal.VerifyAll();
            _mockSynchronizedObjectInstanceCache.VerifyAll();
        }

        [Fact]
        public void RefreshCache_AddsVariantsToCache()
        { 
            string expectedLanguage = "es-ES";
            var expectedTests = new List<IMarketingTest>()
            {
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(), 
                    ContentLanguage = expectedLanguage, State = TestState.Active, 
                    Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } },
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(),
                    ContentLanguage = expectedLanguage, State = TestState.Active, 
                    Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() }  } }
            };

            var expectedTestCriteria = new TestCriteria();
            expectedTestCriteria.AddFilter(
                new ABTestFilter
                {
                    Property = ABTestProperty.State,
                    Operator = FilterOperator.And,
                    Value = TestState.Active
                }
            );
            var expectedContent1 = new Mock<IContent>();
            var expectedContent2 = new Mock<IContent>();

            _mockTestManager.Setup(tm => tm.GetTestList(It.Is<TestCriteria>(tc =>
                                                AssertTestCriteria.AreEquivalent(expectedTestCriteria, tc))))
                                                .Returns(expectedTests);
            _mockTestManager.Setup(tm => tm.GetVariantContent(expectedTests[0].OriginalItemId, It.IsAny<CultureInfo>()))
                                    .Returns(expectedContent1.Object);
            _mockTestManager.Setup(tm => tm.GetVariantContent(expectedTests[1].OriginalItemId, It.IsAny<CultureInfo>()))
                                     .Returns(expectedContent2.Object);

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Remove(CachingTestManager.MasterCacheKey));
            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.GetCacheKeyForVariant(
                                                                        expectedTests[0].OriginalItemId,
                                                                        expectedLanguage),
                                                                        expectedContent1.Object,
                                                                        It.Is<CacheEvictionPolicy>(actual =>
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(null,
                                                                            new string[] { CachingTestManager.MasterCacheKey }), actual))));

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.GetCacheKeyForVariant(
                                                                        expectedTests[1].OriginalItemId,
                                                                        expectedLanguage),
                                                                        expectedContent2.Object,
                                                                        It.Is<CacheEvictionPolicy>(actual =>
                                                                            AssertCacheEvictionPolicy.AreEquivalent(
                                                                                new CacheEvictionPolicy(null,
                                                                                new string[] { CachingTestManager.MasterCacheKey }), actual))));

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.AllTestsKey,
                                                                     expectedTests,
                                                                     It.Is<CacheEvictionPolicy>(actual =>
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(null,
                                                                            new string[] { CachingTestManager.MasterCacheKey }), actual))));

            _mockRemoteCacheSignal.Setup(c => c.Set()).Verifiable();

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object,
                                                 _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            _mockRemoteCacheSignal.ResetCalls();

            manager.RefreshCache();

            // Verify all expected items where put in cache
            _mockSynchronizedObjectInstanceCache.VerifyAll();
        }

        [Fact]
        public void GetActiveTests_ReturnsCopiedList()
        { 
            var originalTestList = new List<IMarketingTest>();
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>()))
                            .Returns(originalTestList);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object,
                                                  _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            _mockRemoteCacheSignal.ResetCalls();

            var actualList = manager.GetActiveTests();

            // make sure we have a copy of the list in mem
            originalTestList.Add(new ABTest { });
            Assert.NotEqual(originalTestList, actualList);
        }

        [Fact]
        public void Start_AddsTestToCache()
        {
            string expectedLanguage = "es-ES";
            var expectedTest = 
                new ABTest { Id = Guid.NewGuid(), OriginalItemId = Guid.NewGuid(),
                    ContentLanguage = expectedLanguage, State = TestState.Active,
                    Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } } };
            var expectedContent = new Mock<IContent>();
            var expectedTests = new List<IMarketingTest> { expectedTest };

            _mockTestManager.Setup(tm => tm.Start(It.IsAny<Guid>())).Returns(expectedTest);
            _mockTestManager.Setup(tm => tm.GetVariantContent(expectedTest.OriginalItemId, It.IsAny<CultureInfo>()))
                            .Returns(expectedContent.Object);

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());
            _mockSynchronizedObjectInstanceCache.Setup(c => c.Get(CachingTestManager.AllTestsKey)).Returns(new List<IMarketingTest>());
            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.GetCacheKeyForVariant(
                                                                        expectedTest.OriginalItemId,
                                                                        expectedLanguage),
                                                                        expectedContent.Object,
                                                                        It.Is<CacheEvictionPolicy>(actual =>
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(null,
                                                                            new string[] { CachingTestManager.MasterCacheKey }), actual))));

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.AllTestsKey,
                                                                     expectedTests,
                                                                     It.Is<CacheEvictionPolicy>(actual =>
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(null,
                                                                            new string[] { CachingTestManager.MasterCacheKey }), actual))));

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object,
                                                 _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            _mockRemoteCacheSignal.ResetCalls();

            manager.Start(expectedTest.Id);
        }

        [Fact]
        public void CachingTestManager_CanHandleManyRequestsInParallel()
        {
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            
            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockRemoteCacheSignal.Object, _mockConfigurationSignal.Object, _mockEvents.Object, _mockTestManager.Object);

            var iterations = 10000;

            Thread addManyTests = new Thread(
                () =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        var testToAdd = new ABTest
                        {
                            Id = Guid.NewGuid(),
                            OriginalItemId = Guid.NewGuid(),
                            ContentLanguage = "es-ES",
                            State = TestState.Active,
                            Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
                        };

                        manager.Save(testToAdd);
                    }
                }
            );

            var testToAdd2 = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                ContentLanguage = "es-ES",
                State = TestState.Active,
                Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
            };
            Thread addManySameTests = new Thread(
                 () =>
                 {
                     for (int i = 0; i < iterations; i++)
                     {
                          manager.Save(testToAdd2);
                     }
                 }
             );

            Thread deleteManyTests = new Thread(
                () =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        manager.Delete(Guid.NewGuid());
                    }
                }
            );

            Thread refreshManyTimes = new Thread(
                () =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        manager.RefreshCache();
                    }
                }
            );

            _mockTestManager.ResetCalls();

            addManyTests.Start();
            deleteManyTests.Start();
            refreshManyTimes.Start();
            addManySameTests.Start();

            Assert.True(addManyTests.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");
            Assert.True(deleteManyTests.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");
            Assert.True(refreshManyTimes.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");
            Assert.True(addManySameTests.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");

            _mockTestManager.Verify(tm => tm.Save(It.IsAny<IMarketingTest>()), Times.Exactly(iterations * 2));
            _mockTestManager.Verify(tm => tm.Delete(It.IsAny<Guid>(), It.IsAny<CultureInfo>()), Times.Exactly(iterations));
            _mockTestManager.Verify(tm => tm.GetTestList(It.IsAny<TestCriteria>()), Times.Exactly(iterations));
        }
    }
}
