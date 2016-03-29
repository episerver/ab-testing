﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;
using ABTest = EPiServer.Marketing.Testing.Data.ABTest;

namespace EPiServer.Marketing.Testing.Test.Core
{
        public class MultivariateManagerTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<ITestingDataAccess> _dataAccessLayer;

        private TestManager GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _dataAccessLayer = new Mock<ITestingDataAccess>();
            _serviceLocator.Setup(sl => sl.GetInstance<ITestingDataAccess>()).Returns(_dataAccessLayer.Object);

            return new TestManager(_serviceLocator.Object);
        }

        private Testing.Dal.EntityModel.DalABTest GetDalTest()
        {
            return new Testing.Dal.EntityModel.DalABTest()
            {
                Variants = new List<DalVariant>()
                {
                    new DalVariant() {Id = Guid.NewGuid(), ItemId = Guid.NewGuid(), ItemVersion = 1}
                },
                KeyPerformanceIndicators = new List<DalKeyPerformanceIndicator>()
                {
                    new DalKeyPerformanceIndicator() { Id = Guid.NewGuid(), KeyPerformanceIndicatorId = Guid.NewGuid() }
                },
                TestResults = new List<DalTestResult>()
                {
                    new DalTestResult() { Id = Guid.NewGuid(), ItemId = Guid.NewGuid(), ItemVersion = 1 }
                }
            };
        }

        private ABTest GetManagerTest()
        {
            return new ABTest()
            {
                Variants = new List<Testing.Data.Variant>(),
                KeyPerformanceIndicators = new List<Data.KeyPerformanceIndicator>(),
                TestResults = new List<Data.TestResult>()
            };
        }

        [Fact]
        public void TestManager_CallsDataAccessGetWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            _dataAccessLayer.Setup(dal => dal.Get(It.IsAny<Guid>())).Returns(GetDalTest());
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
            dalList.Add(GetDalTest());
            _dataAccessLayer.Setup(dal => dal.GetTestByItemId(It.IsAny<Guid>())).Returns(dalList);
            tm.GetTestByItemId(theGuid);

            _dataAccessLayer.Verify(da => da.GetTestByItemId(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer GetTestByItemId was never called or Guid did not match.");
        }

        [Fact]
        public void TestManager_CallsGetTestListWithCritera()
        {
            var critera = new Testing.Data.TestCriteria();
            var testFilter = new Testing.Data.ABTestFilter() { Operator = Testing.Data.FilterOperator.And, Property = Testing.Data.ABTestProperty.OriginalItemId, Value = "Test" };
            critera.AddFilter(testFilter);
            var tm = GetUnitUnderTest();
            var dalList = new List<IABTest>();
            dalList.Add(GetDalTest());
            _dataAccessLayer.Setup(dal => dal.GetTestList(It.IsAny<DalTestCriteria>())).Returns(dalList);
            tm.GetTestList(critera);

            _dataAccessLayer.Verify(da => da.GetTestList(It.Is<DalTestCriteria>(arg => arg.GetFilters().First().Operator == DalFilterOperator.And &&
            arg.GetFilters().First().Property == DalABTestProperty.OriginalItemId &&
            arg.GetFilters().First().Value == testFilter.Value)),
            "DataAcessLayer GetTestList was never called or criteria did not match.");
        }

        [Fact]
        public void TestManager_CallsDeleteWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.Delete(theGuid);

            _dataAccessLayer.Verify(da => da.Delete(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer Delete was never called or Guid did not match.");
        }

        [Fact]
        public void TestManager_CallsStartWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.Start(theGuid);

            _dataAccessLayer.Verify(da => da.Start(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer Start was never called or Guid did not match.");
        }

        [Fact]
        public void TestManager_CallsStopWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.Stop(theGuid);

            _dataAccessLayer.Verify(da => da.Stop(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer Stop was never called or Guid did not match.");
        }

        [Fact]
        public void TestManager_CallsArchiveWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.Archive(theGuid);

            _dataAccessLayer.Verify(da => da.Archive(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer Archive was never called or Guid did not match.");
        }

        [Fact]
        public void TestManager_CallsSaveWithProperArguments()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            ABTest test = new ABTest()
            {
                Id = theGuid,
                ModifiedDate = DateTime.UtcNow,
                Variants = new List<Data.Variant>() { new Data.Variant() { Id = Guid.NewGuid(), ItemId = Guid.NewGuid(), ItemVersion = 1 } },
                KeyPerformanceIndicators = new List<Data.KeyPerformanceIndicator>() { new Data.KeyPerformanceIndicator() { Id = Guid.NewGuid(), KeyPerformanceIndicatorId = Guid.NewGuid() } },
                TestResults = new List<Data.TestResult>() { new Data.TestResult() { Id = Guid.NewGuid(), ItemId = Guid.NewGuid(), ItemVersion = 1 } }
            };
            tm.Save(test);

            _dataAccessLayer.Verify(da => da.Save(It.Is<Testing.Dal.EntityModel.DalABTest>(arg => arg.Id == theGuid)),
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

            _dataAccessLayer.Verify(da => da.IncrementCount(It.Is<Guid>(arg => arg.Equals(theGuid)), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Testing.Dal.EntityModel.Enums.DalCountType>()),
                "DataAcessLayer IncrementCount was never called or Test Guid did not match.");
            _dataAccessLayer.Verify(da => da.IncrementCount(It.IsAny<Guid>(), It.Is<Guid>(arg => arg.Equals(theTestItemGuid)), It.IsAny<int>(), It.IsAny<Testing.Dal.EntityModel.Enums.DalCountType>()),
                "DataAcessLayer IncrementCount was never called or test item Guid did not match.");
            _dataAccessLayer.Verify(da => da.IncrementCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.Is<Testing.Dal.EntityModel.Enums.DalCountType>(arg => arg.Equals(Testing.Dal.EntityModel.Enums.DalCountType.Conversion))),
                "DataAcessLayer IncrementCount was never called or CountType did not match.");
        }

        [Fact]
        public void TestManager_ReturnLandingPage_NoCache()
        {
            // Make sure that the return landing page, calls data access layer if its not in the cache..
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var originalItemId = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A4");
            var vID = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A5");
            var variantList = new List<DalVariant>() { new DalVariant { Id = vID }, new DalVariant {Id = originalItemId} };

            var tm = GetUnitUnderTest();
            _dataAccessLayer.Setup(da => da.Get(It.Is<Guid>(arg => arg.Equals(theGuid)))).Returns(
                new Testing.Dal.EntityModel.DalABTest()
                {
                    Id = theGuid,
                    OriginalItemId = originalItemId,
                    Variants = variantList
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

                if (landingPage == originalItemId && !originalCalled)
                {
                    count++;
                    originalCalled = true;
                }

                if (landingPage == vID && !variantCalled)
                {
                    count++;
                    variantCalled = true;
                }

                _dataAccessLayer.Verify(da => da.Get(It.Is<Guid>(arg => arg.Equals(theGuid))),
                    "DataAcessLayer get was never called or Guid did not match.");
                Assert.True(landingPage.Equals(originalItemId) ||
                              landingPage.Equals(vID), "landingPage is not the original quid or the variant quid");
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
    }
}