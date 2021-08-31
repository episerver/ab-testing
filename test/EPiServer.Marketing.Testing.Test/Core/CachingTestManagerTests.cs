using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Test.Asserts;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Core
{
    public class CachingTestManagerTests
    {
        private Mock<DefaultMarketingTestingEvents> _mockEvents;
        private Mock<ITestManager> _mockTestManager;
        private List<IMarketingTest> _expectedTests;
        private Mock<ISynchronizedObjectInstanceCache> _mockSynchronizedObjectInstanceCache;
        private Mock<ILogger> _logger;

        [ExcludeFromCodeCoverage]
        public CachingTestManagerTests()
        {
            _mockEvents = new Mock<DefaultMarketingTestingEvents>();
            _mockTestManager = new Mock<ITestManager>();
            _logger = new Mock<ILogger>();

            _mockSynchronizedObjectInstanceCache = new Mock<ISynchronizedObjectInstanceCache>(MockBehavior.Strict);

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
        public void CachingTestManager_EvaluateKPIs_InvokesInnerManager()
        {
            var expectedKpis = new List<IKpi>();
            var expectedResults = new List<IKpiResult>();
            var expectedSender = new object();
            var expectedEventArgs = new EventArgs();

            _mockTestManager.Setup(
                tm =>
                    tm.EvaluateKPIs(expectedKpis, expectedSender, expectedEventArgs)
            ).Returns(expectedResults);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            var actualResults = manager.EvaluateKPIs(expectedKpis, expectedSender, expectedEventArgs);

            _mockTestManager.VerifyAll();

            Assert.Equal(expectedResults, actualResults);
        }

        [Fact]
        public void Get_DeliversTestFromCache()
        {
            var expectedTest = _expectedTests.Last();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(CachingTestManager.AllTestsKey)).Returns(_expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);

            _mockTestManager.ResetCalls();

            var actualTest = manager.Get(expectedTest.Id, true);

            Assert.Equal(expectedTest, actualTest);

            _mockTestManager.Verify(tm => tm.Get(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never());
            _mockSynchronizedObjectInstanceCache.VerifyAll();
        }

        [Fact]
        public void Get_DeliversTestFromInnerManagerOnCacheMiss()
        {
            var expectedTest = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                ContentLanguage = "en-GB",
                Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
            };

            _mockTestManager.Setup(tm => tm.Get(expectedTest.Id, false)).Returns(expectedTest);
            
            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            var actualTest = manager.Get(expectedTest.Id, false);

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

             _mockTestManager.Setup(tm => tm.Get(expectedTest.Id, false)).Returns(expectedTest);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            var actualTest = manager.Get(expectedTest.Id, false);

            Assert.Equal(expectedTest, actualTest);

            _mockTestManager.VerifyAll();
        }

        [Fact]
        public void GetsActiveTestsByOriginalItemId_DeliversActiveTest()
        {
            var expectedItem = _expectedTests.First();
            var expectedItemId = expectedItem.OriginalItemId;

            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(CachingTestManager.AllTestsKey)).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            var actualTests = manager.GetActiveTestsByOriginalItemId(expectedItemId);

            Assert.True(actualTests.Count == 1);
            Assert.Equal(expectedItem, actualTests.First());
        }

        [Fact]
        public void CachingTestManager_GetActiveTestsByOriginalItemId_DeliversEmptyListWhenTestNotFound()
        {
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(CachingTestManager.AllTestsKey)).Returns(_expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            var actualTests = manager.GetActiveTestsByOriginalItemId(Guid.NewGuid());

            Assert.True(actualTests.Count == 0);
            _mockSynchronizedObjectInstanceCache.VerifyAll();
        }

        [Fact]
        public void GetsActiveTestsByOriginalItemId_WithCulture_DeliversActiveTest()
        {
            var expectedItem = _expectedTests.First();
            var expectedItemId = expectedItem.OriginalItemId;
            var expectedContentLanguage = expectedItem.ContentLanguage;
            var expectedCulture = CultureInfo.GetCultureInfo(expectedContentLanguage);

            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(CachingTestManager.AllTestsKey)).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            var actualTests = manager.GetActiveTestsByOriginalItemId(expectedItemId, expectedCulture);

            Assert.True(actualTests.Count == 1);
            Assert.Equal(expectedItem, actualTests.First());
        }

        [Fact]
        public void CachingTestManager_GetActiveTestsByOriginalItemId_WithCulture_DeliversEmptyListWhenTestNotFound()
        {
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(CachingTestManager.AllTestsKey)).Returns(_expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
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

            _mockTestManager.Setup(tm => tm.GetDatabaseVersion(expectedDbConnection, expectedSchema, expectedContextKey, expectedPopulateCache)).Returns(100L);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            var actualVersion = manager.GetDatabaseVersion(expectedDbConnection, expectedSchema, expectedContextKey, expectedPopulateCache);

            _mockTestManager.VerifyAll();

            Assert.Equal(100L, actualVersion);
        }

        [Fact]
        public void CachingTestManager_GetTestByItemId_InvokesInnerManager()
        {
            var expectedItemId = Guid.NewGuid();
            var expectedTests = new List<IMarketingTest>();

             _mockTestManager.Setup(tm => tm.GetTestByItemId(expectedItemId)).Returns(expectedTests);
            
            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            var actualTests = manager.GetTestByItemId(expectedItemId);

            _mockTestManager.VerifyAll();

            Assert.Equal(expectedTests, actualTests);
        }

        [Fact]
        public void CachingTestManager_GetTestList_InvokesInnerManager()
        {
            var expectedCriteria = new TestCriteria();
            var expectedTests = new List<IMarketingTest>();

            _mockTestManager.Setup(tm => tm.GetTestList(expectedCriteria)).Returns(expectedTests);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            var actualTests = manager.GetTestList(expectedCriteria);

            _mockTestManager.VerifyAll();

            Assert.Equal(expectedTests, actualTests);
        }

        [Fact]
        public void GetVariantContent_WithCulture_DeliversFromCache()
        {
            var expectedTest = _expectedTests.First();
            var expectedItemId = expectedTest.OriginalItemId;
            var expectedContentLanguage = expectedTest.ContentLanguage;
            var expectedCulture = CultureInfo.GetCultureInfo(expectedContentLanguage);
            var expectedVariant = Mock.Of<IContent>();
            var expectedKey = CachingTestManager.GetCacheKeyForVariant(expectedItemId, expectedContentLanguage);

            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(CachingTestManager.AllTestsKey)).Returns(_expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(expectedKey)).Returns(expectedVariant);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            var actualVariant = manager.GetVariantContent(expectedItemId, expectedCulture);

            Assert.Equal(expectedVariant, actualVariant);
        }

        [Fact]
        public void GetVariantContent_DoesNotAddNullVariantToCache()
        {
            var expectedTest = _expectedTests.First();
            var expectedItemId = expectedTest.OriginalItemId;
            var expectedContentLanguage = expectedTest.ContentLanguage;
            var expectedCulture = CultureInfo.GetCultureInfo(expectedContentLanguage);
            var expectedVariant = Mock.Of<IContent>();
            var expectedKey = CachingTestManager.GetCacheKeyForVariant(expectedItemId, expectedContentLanguage);

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(CachingTestManager.AllTestsKey)).Returns(_expectedTests);

            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(expectedKey)).Returns(null);
            _mockTestManager.Setup(tm => tm.GetVariantContent(expectedItemId, expectedCulture)).Returns((IContent)null);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            _mockSynchronizedObjectInstanceCache.ResetCalls();

            var actualVariant = manager.GetVariantContent(expectedItemId, expectedCulture);

            Assert.Null(actualVariant);
            _mockSynchronizedObjectInstanceCache.Verify(m => m.Insert(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CacheEvictionPolicy>()), Times.Never);
        }

        [Fact]
        public void GetVariantContent_AddsVariantToCache()
        {
            var expectedTest = _expectedTests.First();
            var expectedItemId = expectedTest.OriginalItemId;
            var expectedContentLanguage = expectedTest.ContentLanguage;
            var expectedCulture = CultureInfo.GetCultureInfo(expectedContentLanguage);
            var expectedVariant = Mock.Of<IContent>();
            var expectedKey = CachingTestManager.GetCacheKeyForVariant(expectedItemId, expectedContentLanguage);

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetVariantContent(expectedItemId, expectedCulture)).Returns(expectedVariant);

            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(CachingTestManager.AllTestsKey)).Returns(_expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(expectedKey)).Returns(null);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Insert(expectedKey, expectedVariant, It.Is<CacheEvictionPolicy>(p => p.Expiration.TotalMinutes == 30)));

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            _mockSynchronizedObjectInstanceCache.ResetCalls();

            var actualVariant = manager.GetVariantContent(expectedItemId, expectedCulture);
            Assert.Equal(expectedVariant, actualVariant);
        }

        [Fact]
        public void CachingTestManager_IncrementCount_WithCriteria_InvokesInnerManager()
        {
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var expectedCriteria = new IncrementCountCriteria();            
            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
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

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            manager.IncrementCount(expectedTestId, expectedItemVersion, expectedResultType, expectedKpiId, expectedAsync);

            _mockTestManager.Verify(tm => tm.IncrementCount(expectedTestId, expectedItemVersion, expectedResultType, expectedKpiId, expectedAsync), Times.Once());
        }

        [Fact]
        public void CachingTestManager_ReturnLandingPage_InvokesInnerManager()
        {
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);

            var expectedTestId = Guid.NewGuid();
            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            manager.ReturnLandingPage(expectedTestId);

            _mockTestManager.Verify(tm => tm.ReturnLandingPage(expectedTestId), Times.Once());
        }

        [Fact]
        public void Save_CachesTestIfActive()
        {
            var expectedTest = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                ContentLanguage = "en-GB",
                State = TestState.Active,
                Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
            };
            var expectedContent = new Mock<IContent>();

            _mockTestManager.Setup(tm => tm.Save(expectedTest)).Returns(expectedTest.Id);
            _mockTestManager.Setup(tm => tm.GetVariantContent(expectedTest.OriginalItemId, It.IsAny<CultureInfo>()))
                            .Returns(expectedContent.Object);

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Get(CachingTestManager.AllTestsKey)).Returns(_expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.AllTestsKey,
                                                                    It.IsAny<List<IMarketingTest>>(),
                                                                    It.IsAny<CacheEvictionPolicy>()));

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.GetCacheKeyForVariant(
                                                                        expectedTest.OriginalItemId,
                                                                        expectedTest.ContentLanguage),
                                                                        expectedContent.Object,
                                                                        It.Is<CacheEvictionPolicy>(actual =>
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(new TimeSpan(0, 30, 0),
                                                                                                    CacheTimeoutType.Absolute, 
                                                                                                    null,
                                                                                                    new string[] { CachingTestManager.MasterCacheKey }), 
                                                                                                    actual))));

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);

            var actualTestId = manager.Save(expectedTest);

            Assert.Equal(expectedTest.Id, actualTestId);

            _mockEvents.Verify(
                e => 
                    e.RaiseMarketingTestingEvent(
                        DefaultMarketingTestingEvents.TestAddedToCacheEvent, 
                        It.Is<TestEventArgs>(args => args.Test == expectedTest)
                    ), 
                Times.Once
            );

            _mockTestManager.VerifyAll();
            _mockSynchronizedObjectInstanceCache.VerifyAll();
        }

        [Fact]
        public void Save_DoesntCacheTestIfAlreadyInCache()
        {
            var expectedTest = new ABTest
            {
                Id = Guid.NewGuid(),
                OriginalItemId = Guid.NewGuid(),
                ContentLanguage = "en-GB",
                State = TestState.Active,
                Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
            };
            var expectedContent = new Mock<IContent>();
            var expectedTests = new List<IMarketingTest> { expectedTest };
            _mockTestManager.Setup(tm => tm.Save(expectedTest)).Returns(expectedTest.Id);

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Get(CachingTestManager.AllTestsKey)).Returns(expectedTests);


            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);

            _mockEvents.ResetCalls();

            var actualTestId = manager.Save(expectedTest);

            Assert.Equal(expectedTest.Id, actualTestId);

            _mockEvents.Verify(
                e =>
                    e.RaiseMarketingTestingEvent(
                        DefaultMarketingTestingEvents.TestAddedToCacheEvent,
                        It.Is<TestEventArgs>(args => args.Test == expectedTest)
                    ),
                Times.Never
            );

            _mockTestManager.VerifyAll();
            _mockSynchronizedObjectInstanceCache.VerifyAll();
        }

        [Fact]
        public void Save_RemovesFromCacheIfNotActive_SendsMessageToReset()
        {
            var expectedTest = _expectedTests.First();

            _mockTestManager.Setup(tm => tm.Save(expectedTest)).Returns(expectedTest.Id);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Get(CachingTestManager.AllTestsKey)).Returns(_expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Insert(CachingTestManager.AllTestsKey,It.IsAny<object>(),It.IsAny<CacheEvictionPolicy>() ));
            var expectedKey = CachingTestManager.GetCacheKeyForVariant(expectedTest.OriginalItemId, expectedTest.ContentLanguage);
            _mockSynchronizedObjectInstanceCache.Setup(call => call.RemoveLocal(expectedKey));

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);

            expectedTest.State = TestState.Inactive;
            var actualTestId = manager.Save(expectedTest);

            Assert.Equal(expectedTest.Id, actualTestId);

            _mockTestManager.VerifyAll();

            _mockEvents.Verify( e =>
                                e.RaiseMarketingTestingEvent(
                                    DefaultMarketingTestingEvents.TestRemovedFromCacheEvent,
                                    It.Is<TestEventArgs>(args => args.Test == expectedTest)
                                ),
                                Times.Once // once when removed
             );
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

            _mockTestManager.Setup(tm => tm.Start(expectedTest.Id)).Returns(expectedTest);

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);

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
        }

        [Fact]
        public void RefreshCache_BuildsCache_AndUsesExpectedCacheTimeout()
        {
            var expectedVariant = Mock.Of<IContent>();
            var expectedTests = new List<IMarketingTest>()
            {
                new ABTest { Id = new Guid("1C46828D-7F2E-4569-9D8E-2B5F7DF1CFE5"), OriginalItemId = new Guid("1C46828D-7F2E-4569-9D8E-2B5F7DF1CFE6"), ContentLanguage = "es-ES", 
                    State = TestState.Active, Variants = new List<Variant> { new Variant { Id = new Guid("1C46828D-7F2E-4569-9D8E-2B5F7DF1CFE6") }  } },
                new ABTest { Id = new Guid("1C46828D-7F2E-4569-9D8E-2B5F7DF1CFE7"), OriginalItemId = new Guid("1C46828D-7F2E-4569-9D8E-2B5F7DF1CFE8"), ContentLanguage = "es-ES", 
                    State = TestState.Active, Variants = new List<Variant> { new Variant { Id = new Guid("1C46828D-7F2E-4569-9D8E-2B5F7DF1CFE8") }  } }
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
            var expectedTimeout = 90;

            _mockTestManager.Setup(tm => tm.GetTestList(It.Is<TestCriteria>(tc =>
                                                AssertTestCriteria.AreEquivalent(expectedTestCriteria, tc))))
                                                .Returns(expectedTests);

            _mockTestManager.Setup(tm => tm.GetVariantContent(expectedTests[0].OriginalItemId, CultureInfo.GetCultureInfo(expectedTests[0].ContentLanguage))).Returns(expectedVariant);
            _mockTestManager.Setup(tm => tm.GetVariantContent(expectedTests[1].OriginalItemId, CultureInfo.GetCultureInfo(expectedTests[1].ContentLanguage))).Returns(expectedVariant);

            _mockSynchronizedObjectInstanceCache.Setup(c => c.RemoveLocal(CachingTestManager.MasterCacheKey));
            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.AllTestsKey,
                                                                     expectedTests,
                                                                     It.Is<CacheEvictionPolicy>(actual => 
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(new TimeSpan(0, expectedTimeout, 0),
                                                                                                    CacheTimeoutType.Absolute, 
                                                                                                    null, 
                                                                                                    new string[] { CachingTestManager.MasterCacheKey }),
                                                                                                    actual))));

            _mockSynchronizedObjectInstanceCache.Setup(call => call.Insert(It.IsAny<string>(), It.IsAny<object>(), It.Is<CacheEvictionPolicy>(p => p.Expiration.TotalMinutes == expectedTimeout)));
            _mockSynchronizedObjectInstanceCache.Setup(call => call.Insert(It.IsAny<string>(), It.IsAny<object>(), It.Is<CacheEvictionPolicy>(p => p.Expiration.TotalMinutes == expectedTimeout)));

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object, expectedTimeout);
            _mockEvents.ResetCalls();

            manager.RefreshCache();

            _mockTestManager.VerifyAll();
            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestAddedToCacheEvent, It.IsAny<TestEventArgs>()), Times.Exactly(expectedTests.Count));
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

            _mockSynchronizedObjectInstanceCache.Setup(c => c.RemoveLocal(CachingTestManager.MasterCacheKey));
            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.GetCacheKeyForVariant(
                                                                        expectedTests[0].OriginalItemId,
                                                                        expectedLanguage),
                                                                        expectedContent1.Object,
                                                                        It.Is<CacheEvictionPolicy>(actual =>
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(new TimeSpan(0, 30, 0), 
                                                                                                    CacheTimeoutType.Absolute, 
                                                                                                    null,
                                                                                                    new string[] { CachingTestManager.MasterCacheKey }),
                                                                                                    actual))));

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.GetCacheKeyForVariant(
                                                                        expectedTests[1].OriginalItemId,
                                                                        expectedLanguage),
                                                                        expectedContent2.Object,
                                                                        It.Is<CacheEvictionPolicy>(actual =>
                                                                            AssertCacheEvictionPolicy.AreEquivalent(
                                                                                new CacheEvictionPolicy(new TimeSpan(0, 30, 0),
                                                                                                        CacheTimeoutType.Absolute, 
                                                                                                        null,
                                                                                                        new string[] { CachingTestManager.MasterCacheKey }),
                                                                                                        actual))));

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.AllTestsKey,
                                                                     expectedTests,
                                                                     It.Is<CacheEvictionPolicy>(actual =>
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(new TimeSpan(0, 30, 0),
                                                                                                    CacheTimeoutType.Absolute, 
                                                                                                    null,
                                                                                                    new string[] { CachingTestManager.MasterCacheKey }), 
                                                                                                    actual))));

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);

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

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Get(CachingTestManager.AllTestsKey)).Returns(originalTestList);

            var actualList = manager.GetActiveTests();

            // make sure we have a copy of the list in mem
            originalTestList.Add(new ABTest { });
            Assert.NotEqual(originalTestList, actualList);
        }

        [Fact]
        public void GetActiveTests_RefreshesCache_IfNotFound()
        {
            var originalTestList = new List<IMarketingTest>();
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>()))
                            .Returns(originalTestList);

            _mockSynchronizedObjectInstanceCache.SetupSequence(c => c.Get(CachingTestManager.AllTestsKey))
                                                .Returns(null)
                                                .Returns(originalTestList);
            _mockSynchronizedObjectInstanceCache.Setup(m => m.RemoveLocal(CachingTestManager.MasterCacheKey));

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.AllTestsKey,
                                                                     It.IsAny<List<IMarketingTest>>(), 
                                                                     It.IsAny<CacheEvictionPolicy>()));

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object, 30);
            var actualList = manager.GetActiveTests();

            _mockTestManager.VerifyAll();
            _mockSynchronizedObjectInstanceCache.VerifyAll();
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
                                                                            new CacheEvictionPolicy(new TimeSpan(0, 30, 0),
                                                                                                    CacheTimeoutType.Absolute,
                                                                                                    null,
                                                                                                    new string[] { CachingTestManager.MasterCacheKey }), 
                                                                                                    actual))));

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.AllTestsKey,
                                                                     expectedTests,
                                                                     It.Is<CacheEvictionPolicy>(actual =>
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(new TimeSpan(0, 30, 0),
                                                                                                    CacheTimeoutType.Absolute, 
                                                                                                    null,
                                                                                                    new string[] { CachingTestManager.MasterCacheKey }), 
                                                                                                    actual))));

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            manager.Start(expectedTest.Id);

            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestAddedToCacheEvent, It.IsAny<TestEventArgs>()), Times.Once);
        }

        [Fact]
        public void Stop_RemovesTestFromCache()
        {
            string expectedLanguage = "es-ES";
            var expectedTest =
                new ABTest
                {
                    Id = Guid.NewGuid(),
                    OriginalItemId = Guid.NewGuid(),
                    ContentLanguage = expectedLanguage,
                    State = TestState.Active,
                    Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
                };
            var expectedContent = new Mock<IContent>();
            var expectedTests = new List<IMarketingTest> { expectedTest };

            _mockTestManager.Setup(tm => tm.Stop(It.IsAny<Guid>(), It.IsAny<CultureInfo>()));
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(expectedTests);

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Get(CachingTestManager.AllTestsKey)).Returns(expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(c => c.RemoveLocal(CachingTestManager.GetCacheKeyForVariant(
                                                                        expectedTest.OriginalItemId,
                                                                        expectedLanguage)));

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.AllTestsKey,
                                                                     new List<IMarketingTest>(),
                                                                     It.Is<CacheEvictionPolicy>(actual =>
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(new TimeSpan(0, 30, 0),
                                                                                                    CacheTimeoutType.Absolute, 
                                                                                                    null,
                                                                                                    new string[] { CachingTestManager.MasterCacheKey }), 
                                                                                                    actual))));

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            manager.Stop(expectedTest.Id);

            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, It.IsAny<TestEventArgs>()), Times.Once);
        }

        [Fact]
        public void Archive_RemovesTestFromCache()
        {
            string expectedLanguage = "es-ES";
            var expectedTest =
                new ABTest
                {
                    Id = Guid.NewGuid(),
                    OriginalItemId = Guid.NewGuid(),
                    ContentLanguage = expectedLanguage,
                    State = TestState.Active,
                    Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
                };
            var expectedContent = new Mock<IContent>();
            var expectedTests = new List<IMarketingTest> { expectedTest };

            _mockTestManager.Setup(tm => tm.Archive(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CultureInfo>()));
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(expectedTests);

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Get(CachingTestManager.AllTestsKey)).Returns(expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(c => c.RemoveLocal(CachingTestManager.GetCacheKeyForVariant(
                                                                        expectedTest.OriginalItemId,
                                                                        expectedLanguage)));

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.AllTestsKey,
                                                                     new List<IMarketingTest>(),
                                                                     It.Is<CacheEvictionPolicy>(actual =>
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(new TimeSpan(0, 30, 0),
                                                                                                    CacheTimeoutType.Absolute, 
                                                                                                    null,
                                                                                                    new string[] { CachingTestManager.MasterCacheKey }), 
                                                                                                    actual))));

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            manager.Archive(expectedTest.Id, expectedTest.Variants.First().Id);

            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, It.IsAny<TestEventArgs>()), Times.Once);
        }

        [Fact]
        public void Delete_RemovesTestFromCache()
        {
            string expectedLanguage = "es-ES";
            var expectedTest =
                new ABTest
                {
                    Id = Guid.NewGuid(),
                    OriginalItemId = Guid.NewGuid(),
                    ContentLanguage = expectedLanguage,
                    State = TestState.Active,
                    Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
                };
            var expectedContent = new Mock<IContent>();
            var expectedTests = new List<IMarketingTest> { expectedTest };

            _mockTestManager.Setup(tm => tm.Delete(It.IsAny<Guid>(), It.IsAny<CultureInfo>()));
            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(expectedTests);

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Get(CachingTestManager.AllTestsKey)).Returns(expectedTests);
            _mockSynchronizedObjectInstanceCache.Setup(c => c.RemoveLocal(CachingTestManager.GetCacheKeyForVariant(
                                                                        expectedTest.OriginalItemId,
                                                                        expectedLanguage)));

            _mockSynchronizedObjectInstanceCache.Setup(c => c.Insert(CachingTestManager.AllTestsKey,
                                                                     new List<IMarketingTest>(),
                                                                     It.Is<CacheEvictionPolicy>(actual =>
                                                                        AssertCacheEvictionPolicy.AreEquivalent(
                                                                            new CacheEvictionPolicy(new TimeSpan(0,30,0), CacheTimeoutType.Absolute,
                                                                            null,
                                                                            new string[] { CachingTestManager.MasterCacheKey }), actual))));

            var manager = new CachingTestManager(_mockSynchronizedObjectInstanceCache.Object, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            manager.Delete(expectedTest.Id);

            _mockEvents.Verify(e => e.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, It.IsAny<TestEventArgs>()), Times.Once);
            _mockSynchronizedObjectInstanceCache.VerifyAll();
        }

        [Fact]
        public void CachingTestManager_CanHandleManyRequestsInParallel()
        {
            var expectedVariant = Mock.Of<IContent>();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetVariantContent(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(expectedVariant);

            var cache = new MyCache();
            cache.Clear();

            var manager = new CachingTestManager(cache, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            _mockTestManager.ResetCalls();

            var iterations = 500;
            var testIds = new ConcurrentQueue<Guid>();

            Thread addManyTests = new Thread(
                () =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        var testId = Guid.NewGuid();
                        var testToAdd = new ABTest
                        {
                            Id = testId,
                            OriginalItemId = Guid.NewGuid(),
                            ContentLanguage = "es-ES",
                            State = TestState.Active,
                            Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
                        };

                        // this is alot of mocks, whatchout for mem usage. Needs to be mocked so save 
                        // does not return an empty guid which in turn caused the remove from cache to fail all the time
                        _mockTestManager.Setup(tm => tm.Save(testToAdd)).Returns(testToAdd.Id);
                        testIds.Enqueue(manager.Save(testToAdd));
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
                    var x = 0;
                    do
                    {
                        if (testIds.TryDequeue(out Guid testId))
                        {
                            manager.Delete(testId);
                        }
                        else
                        {
                            Thread.Sleep(250);
                            if (!addManyTests.IsAlive)
                            {
                                x++;
                            }
                        }
                    } while (x < 10);
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

            addManyTests.Start();
            addManySameTests.Start();
            deleteManyTests.Start();
            refreshManyTimes.Start();

            Assert.True(addManyTests.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");
            Assert.True(addManySameTests.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");
            Assert.True(deleteManyTests.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");
            Assert.True(refreshManyTimes.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");

            _mockTestManager.Verify(tm => tm.Save(It.IsAny<IMarketingTest>()), Times.Exactly(iterations * 2));
            _mockTestManager.Verify(tm => tm.Delete(It.IsAny<Guid>(), It.IsAny<CultureInfo>()), Times.Exactly(iterations));
        }

        [Fact]
        public void CachingTestManager_CanHandleManyAddAndRemoveInParallel()
        {
            var expectedVariant = Mock.Of<IContent>();

            _mockTestManager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(_expectedTests);
            _mockTestManager.Setup(tm => tm.GetVariantContent(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(expectedVariant);

            var cache = new MyCache();
            cache.Clear();
            var manager = new CachingTestManager(cache, _mockEvents.Object, _mockTestManager.Object, _logger.Object,30);
            var expectedActiveTests = manager.GetActiveTests();

            _mockTestManager.ResetCalls();

            var iterations = 500;
            var testIds = new ConcurrentQueue<Guid>();

            Thread addManyTests = new Thread(
                () =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        var testId = Guid.NewGuid();
                        var testToAdd = new ABTest
                        {
                            Id = testId,
                            OriginalItemId = Guid.NewGuid(),
                            ContentLanguage = "es-ES",
                            State = TestState.Active,
                            Variants = new List<Variant> { new Variant { Id = Guid.NewGuid() } }
                        };

                        // this is alot of mocks, whatchout for mem usage. Needs to be mocked so save 
                        // does not return an empty guid which in turn caused the remove from cache to fail all the time
                        _mockTestManager.Setup(tm => tm.Save(testToAdd)).Returns(testToAdd.Id);
                        _mockTestManager.Setup(tm => tm.Delete(testToAdd.Id, It.IsAny<CultureInfo>()));
                        _mockTestManager.Setup(tm => tm.GetVariantContent(testToAdd.OriginalItemId, It.IsAny<CultureInfo>())).Returns(expectedVariant); 
                        testIds.Enqueue(manager.Save(testToAdd));
                    }
                }
            );

            Thread deleteManyTests = new Thread(
                () =>
                {
                    var x = 0;
                    do
                    {
                        if (testIds.TryDequeue(out Guid testId))
                        {
                            manager.Delete(testId);
                        }
                        else
                        {
                            Thread.Sleep(250);
                            if (!addManyTests.IsAlive)
                            {
                                x++;
                            }
                        }
                    } while (x < 10 );
                }
            );

            addManyTests.Start();
            deleteManyTests.Start();
 
            Assert.True(addManyTests.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");
            Assert.True(deleteManyTests.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");

            // Verify our starting and ending results are the same.
            var actualActiveTests = manager.GetActiveTests();
            Assert.Equal(expectedActiveTests, actualActiveTests);

            // Verify that all the add and delete mocks where hit properly.
            _mockTestManager.VerifyAll(); 

        }

        public class MyCache : ISynchronizedObjectInstanceCache
        {
            public FailureRecoveryAction SynchronizationFailedStrategy
            {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            public IObjectInstanceCache ObjectInstanceCache => new HttpRuntimeCache();

            public void Clear()
            {
                ObjectInstanceCache.Remove(CachingTestManager.MasterCacheKey);
            }

            public object Get(string key)
            {
                return ObjectInstanceCache.Get(key);
            }

            public void Insert(string key, object value, CacheEvictionPolicy evictionPolicy)
            {
                ObjectInstanceCache.Insert(key, value, evictionPolicy);
            }

            public void Remove(string key)
            {
                ObjectInstanceCache.Remove(key);
            }

            public void RemoveLocal(string key)
            {
                ObjectInstanceCache.Remove(key);
            }

            public void RemoveRemote(string key)
            {

            }
        }
    }
}
