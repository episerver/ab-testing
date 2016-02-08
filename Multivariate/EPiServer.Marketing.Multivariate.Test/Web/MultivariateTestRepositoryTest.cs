using EPiServer.Core;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Model.Enums;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing;
using Xunit;

namespace EPiServer.Marketing.Multivariate.Test.Web
{
    [ExcludeFromCodeCoverage]
    public class MultivariateTestRepositoryTest
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<ITestManager> _testmanager;

        static Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
        static Guid original = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE7");
        static Guid varient = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE6");
        static Guid result1 = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE5");
        static Guid result2 = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE4");
        ABTestViewModel viewdata = new ABTestViewModel()
        {
            id = theGuid,
            Title = "Title",
            Owner = Security.PrincipalInfo.CurrentPrincipal.Identity.Name, // Repo / business logic sets it to this.
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(2),
            OriginalItemId = original,
            OriginalItem = 1,
            VariantItem = 2,
            testState = TestState.Active,
            VariantItemId = varient,
            OriginalItemDisplay = "Original Item",
            VariantItemDisplay = "Variant Item",
            TestResults = new List<TestResult>() {
                    new TestResult() { Id = result1, ItemId = original },
                    new TestResult() { Id = result2, ItemId = varient }
                }
        };

        ABTest test = new ABTest()
        {
            Id = theGuid,
            Title = "Title",
            Owner = "Owner",
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(2),
            OriginalItemId = original,
            TestState = TestState.Active,
            Variants = new List<Variant>() { new Variant() { Id = Guid.NewGuid(), VariantId = varient } },
            MultivariateTestResults = new List<TestResult>() {
                    new TestResult() { Id = result1, ItemId = original },
                    new TestResult() { Id = result2, ItemId = varient }
                }
        };

        private DateTime newEndDate = DateTime.Now;

        private TestRepository GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _testmanager = new Mock<ITestManager>();

            // Setup the contentrepo so it simulates episerver returning content
            var page1 = new BasicContent() { ContentGuid = viewdata.OriginalItemId, Name = viewdata.OriginalItemDisplay, ContentLink = new ContentReference() { ID = viewdata.OriginalItem } };
            var page2 = new BasicContent() { ContentGuid = viewdata.VariantItemId, Name = viewdata.VariantItemDisplay, ContentLink = new ContentReference() { ID = viewdata.VariantItem } };

            var contentRepo = new Mock<IContentRepository>();
            viewdata.OriginalItem = 1;
            viewdata.VariantItem = 2;
            contentRepo.Setup(cr => cr.Get<IContent>(It.Is<ContentReference>(cf => cf.ID == 1))).Returns(page1);
            contentRepo.Setup(cr => cr.Get<IContent>(It.Is<ContentReference>(cf => cf.ID == 2))).Returns(page2);
            contentRepo.Setup(cr => cr.Get<IContent>(It.Is<Guid>(x => x == original))).Returns(page1);
            contentRepo.Setup(cr => cr.Get<IContent>(It.Is<Guid>(x => x == varient))).Returns(page2);


            _serviceLocator.Setup(sl => sl.GetInstance<IContentRepository>()).Returns(contentRepo.Object);

            return new TestRepository(_serviceLocator.Object);
        }

        [Fact]
        public void GetWinningTestResult_ReturnsCorrectResult()
        {
            ABTestViewModel test = new ABTestViewModel();

            var results = new List<TestResult>();
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            results.Add(new TestResult() { Views = 100, Conversions = 90 });
            results.Add(new TestResult() { Views = 200, Conversions = 100 });
            results.Add(new TestResult() { Id = theGuid, Views = 300, Conversions = 300 });
            results.Add(new TestResult() { Views = 400, Conversions = 100 });
            results.Add(new TestResult() { Views = 500, Conversions = 200 });
            test.TestResults = results;

            TestRepository repo = GetUnitUnderTest();

            var winner = repo.GetWinningTestResult(test);

            Assert.Equal(theGuid, winner.Id);
        }

        [Fact]
        public void DeleteTestCallsTestManagerDeleteWithProperTestGuid()
        {
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            var repo = GetUnitUnderTest();

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testmanager.Object);

            repo.DeleteTest(theGuid);

            // Verify the testmanager was called by the repo with the proper argument
            _testmanager.Verify(tm => tm.Delete(It.Is<Guid>(guid => guid.Equals(theGuid))),
                Times.Once, "DeleteTest did not call Delete or Delete was not called with expected values");
        }

        [Fact]
        public void StopTestCallsTestManagerStopWithProperTestGuid()
        {
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            var repo = GetUnitUnderTest();

            _serviceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testmanager.Object);

            repo.StopTest(theGuid);

            // verify thes testmanager was called with the proper guid.
            _testmanager.Verify(tm => tm.Stop(It.Is<Guid>(guid => guid.Equals(theGuid))),
                Times.Once(), "StopTest did not call Stop or Stop was not called with expected values");

        }

        [Fact]
        public void GetTestByIdCallsTestManagerWithProperTestGuid()
        {
            var repo = GetUnitUnderTest();

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testmanager.Object);
            _testmanager.Setup(tm => tm.Get(It.IsAny<Guid>())).Returns(test);

            repo.GetTestById(theGuid);

            // Verify the testmanager was called by the repo with the proper argument
            _testmanager.Verify(tm => tm.Get(It.Is<Guid>(guid => guid.Equals(theGuid))),
                Times.Once, "GetTestById did not call Get or Get was not called with expected values");
        }

        [Fact]
        public void GetTestListCallsTestManagerWithProperCriteria()
        {
            var criteria = new TestCriteria();
            var repo = GetUnitUnderTest();

            List<IABTest> list = new List<IABTest>() { test };

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testmanager.Object);
            _testmanager.Setup(tm => tm.GetTestList(It.IsAny<TestCriteria>())).Returns(list);

            repo.GetTestList(criteria);

            // Verify the testmanager was called by the repo with the proper argument
            _testmanager.Verify(tm => tm.GetTestList(It.Is<TestCriteria>(tc => tc.Equals(criteria))),
                Times.Once, "GetTestList did not call TestManager.GetTestList or TestManager.GetTestList was not called with expected values");
        }

        [Fact]
        public void ConvertToViewModelConvertsProperly()
        {
            var repo = GetUnitUnderTest();
            var ViewData = repo.ConvertToViewModel(test);

            // Verify the testmanager was called by the repo with the proper argument
            Assert.Equal(test.Id, ViewData.id);
            Assert.Equal(test.Title, ViewData.Title);
            Assert.Equal(test.Owner, ViewData.Owner);
            Assert.Equal(test.StartDate, ViewData.StartDate);
            Assert.Equal(test.EndDate, ViewData.EndDate);
            Assert.Equal(test.OriginalItemId, ViewData.OriginalItemId);
            Assert.Equal(test.TestState, ViewData.testState);
            Assert.Equal(test.Variants[0].VariantId, ViewData.VariantItemId);
            Assert.Equal(test.MultivariateTestResults[0].Id, ViewData.TestResults[0].Id);
            Assert.Equal(test.MultivariateTestResults[1].Id, ViewData.TestResults[1].Id);
        }

        [Fact]
        public void CreateTestCallsTestRepoSave()
        {
            var repo = GetUnitUnderTest();

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testmanager.Object);

            repo.CreateTest(viewdata);

            // Verify the testmanager was called by the repo with the proper argument
            _testmanager.Verify(tm => tm.Save(It.IsAny<ABTest>()),
                Times.Once, "CreateTest did not call save");
        }

        [Fact]
        public void CreateTest_WithTestStateActive_SetsDateValue()
        {
            var repo = GetUnitUnderTest();

            test.TestState = TestState.Active;
            _serviceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testmanager.Object);
            _testmanager.Setup(tm => tm.Get(It.IsAny<Guid>())).Returns(test);
            viewdata.EndDate = newEndDate;
            repo.CreateTest(viewdata);
            _testmanager.Verify(tm => tm.Save(It.Is<ABTest>(tc => tc.EndDate.Equals(newEndDate))),
               Times.Once, "CreateTest did not call save with correctly altered end date");

        }


        [Fact]
        public void CreateTestCallsTestRepoSaveWithMappedData()
        {
            var repo = GetUnitUnderTest();

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testmanager.Object);

            // now create the test and verify data was mapped properly
            repo.CreateTest(viewdata);

            // Verify the testmanager was called by the repo with the proper arguments - mapped
            _testmanager.Verify(tm => tm.Save(It.Is<ABTest>(tc => tc.Id.Equals(viewdata.id))),
                Times.Once, "CreateTest did not call save with correctly mapped ID");
            _testmanager.Verify(tm => tm.Save(It.Is<ABTest>(tc => tc.Title.Equals(viewdata.Title))),
                Times.Once, "CreateTest did not call save with correctly mapped Title");
            _testmanager.Verify(tm => tm.Save(It.Is<ABTest>(tc => tc.Owner.Equals(viewdata.Owner))),
                Times.Once, "CreateTest did not call save with correctly mapped Owner");
            _testmanager.Verify(tm => tm.Save(It.Is<ABTest>(tc => tc.StartDate.Equals(viewdata.StartDate))),
                Times.Once, "CreateTest did not call save with correctly mapped StartDate");
            _testmanager.Verify(tm => tm.Save(It.Is<ABTest>(tc => tc.EndDate.Equals(viewdata.EndDate))),
                Times.Once, "CreateTest did not call save with correctly mapped EndDate");
            _testmanager.Verify(tm => tm.Save(It.Is<ABTest>(tc => tc.OriginalItemId.Equals(viewdata.OriginalItemId))),
                Times.Once, "CreateTest did not call save with correctly mapped OriginalItemId");
            _testmanager.Verify(tm => tm.Save(It.Is<ABTest>(tc => tc.Variants[0].VariantId.Equals(viewdata.VariantItemId))),
                Times.Once, "CreateTest did not call save with correctly mapped Variants[0].VariantId");
        }
    }
}
