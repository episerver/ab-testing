using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using EPiServer.Marketing.Multivariate.Model;
using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Web.Models;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Core;

namespace EPiServer.Marketing.Multivariate.Test.Web
{
    [TestClass]
    public class MultivariateTestRepositoryTest
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IMultivariateTestManager> _testmanager;

        static Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
        static Guid original = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE7");
        static Guid varient = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE6");
        static Guid result1 = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE5");
        static Guid result2 = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE4");
        MultivariateTestViewModel viewdata = new MultivariateTestViewModel()
        {
            id = theGuid,
            Title = "Title",
            Owner = Security.PrincipalInfo.CurrentPrincipal.Identity.Name, // Repo / business logic sets it to this.
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(2),
            OriginalItemId = original,
            OriginalItem = 1,
            VariantItem = 2,
            testState = Model.Enums.TestState.Active,
            VariantItemId = varient,
            TestResults = new List<MultivariateTestResult>() {
                    new MultivariateTestResult() { Id = result1 },
                    new MultivariateTestResult() { Id = result2 }
                }
        };

        MultivariateTest test = new MultivariateTest()
        {
            Id = theGuid,
            Title = "Title",
            Owner = "Owner",
            StartDate = DateTime.Today.AddDays(1),
            EndDate = DateTime.Today.AddDays(2),
            OriginalItemId = original,
            TestState = Model.Enums.TestState.Active,
            Variants = new List<Variant>() { new Variant() { Id = varient } },
            MultivariateTestResults = new List<MultivariateTestResult>() {
                    new MultivariateTestResult() { Id = result1 },
                    new MultivariateTestResult() { Id = result2 }
                }
        };

        private MultivariateTestRepository GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _testmanager = new Mock<IMultivariateTestManager>();

            // Setup the contentrepo so it simulates episerver returning content
            var page1 = new BasicContent() { ContentGuid = viewdata.OriginalItemId };
            var page2 = new BasicContent() { ContentGuid = viewdata.VariantItemId };

            var contentRepo = new Mock<IContentRepository>();
            viewdata.OriginalItem = 1;
            viewdata.VariantItem = 2;
            contentRepo.Setup(cr => cr.Get<IContent>(It.Is<ContentReference>(cf => cf.ID == 1))).Returns(page1);
            contentRepo.Setup(cr => cr.Get<IContent>(It.Is<ContentReference>(cf => cf.ID == 2))).Returns(page2);
            _serviceLocator.Setup(sl => sl.GetInstance<IContentRepository>()).Returns(contentRepo.Object);

            return new MultivariateTestRepository(_serviceLocator.Object);
        }

        [TestMethod]
        public void GetWinningTestResult_ReturnsCorrectResult()
        {
            MultivariateTestViewModel test = new MultivariateTestViewModel();

            var results = new List<MultivariateTestResult>();
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            results.Add(new MultivariateTestResult() { Views = 100, Conversions = 90 });
            results.Add(new MultivariateTestResult() { Views = 200, Conversions = 100 });
            results.Add(new MultivariateTestResult() { Id = theGuid, Views = 300, Conversions = 300 });
            results.Add(new MultivariateTestResult() { Views = 400, Conversions = 100 });
            results.Add(new MultivariateTestResult() { Views = 500, Conversions = 200 });
            test.TestResults = results;

            MultivariateTestRepository repo = GetUnitUnderTest();

            var winner = repo.GetWinningTestResult(test);

            Assert.AreEqual(theGuid, winner.Id);
        }

        [TestMethod]
        public void DeleteTestCallsTestManagerDeleteWithProperTestGuid()
        {
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            var repo = GetUnitUnderTest();

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestManager>()).Returns(_testmanager.Object);

            repo.DeleteTest(theGuid);

            // Verify the testmanager was called by the repo with the proper argument
            _testmanager.Verify(tm => tm.Delete(It.Is<Guid>(guid => guid.Equals(theGuid))),
                Times.Once, "DeleteTest did not call Delete or Delete was not called with expected values");
        }

        [TestMethod]
        public void StopTestCallsTestManagerStopWithProperTestGuid()
        {
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            var repo = GetUnitUnderTest();

            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestManager>()).Returns(_testmanager.Object);

            repo.StopTest(theGuid);

            // verify thes testmanager was called with the proper guid.
            _testmanager.Verify(tm => tm.Stop(It.Is<Guid>(guid => guid.Equals(theGuid))),
                Times.Once(), "StopTest did not call Stop or Stop was not called with expected values");

        }

        [TestMethod]
        public void GetTestByIdCallsTestManagerWithProperTestGuid()
        {
            var repo = GetUnitUnderTest();

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestManager>()).Returns(_testmanager.Object);
            _testmanager.Setup(tm => tm.Get(It.IsAny<Guid>())).Returns(test);

            repo.GetTestById(theGuid);

            // Verify the testmanager was called by the repo with the proper argument
            _testmanager.Verify(tm => tm.Get(It.Is<Guid>(guid => guid.Equals(theGuid))),
                Times.Once, "GetTestById did not call Get or Get was not called with expected values");
        }

        [TestMethod]
        public void GetTestListCallsTestManagerWithProperCriteria()
        {
            var criteria = new MultivariateTestCriteria();
            var repo = GetUnitUnderTest();

            List<IMultivariateTest> list = new List<IMultivariateTest>() { test };

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestManager>()).Returns(_testmanager.Object);
            _testmanager.Setup(tm => tm.GetTestList(It.IsAny<MultivariateTestCriteria>())).Returns(list);

            repo.GetTestList(criteria);

            // Verify the testmanager was called by the repo with the proper argument
            _testmanager.Verify(tm => tm.GetTestList(It.Is<MultivariateTestCriteria>(tc => tc.Equals(criteria))),
                Times.Once, "GetTestList did not call TestManager.GetTestList or TestManager.GetTestList was not called with expected values");
        }

        [TestMethod]
        public void ConvertToViewModelConvertsProperly()
        {
            var repo = GetUnitUnderTest();
            var ViewData = repo.ConvertToViewModel(test);

            // Verify the testmanager was called by the repo with the proper argument
            Assert.AreEqual(test.Id, ViewData.id, "Test ID was not mapped.");
            Assert.AreEqual(test.Title, ViewData.Title, "Title was not mapped.");
            Assert.AreEqual(test.Owner, ViewData.Owner, "Owner was not mapped.");
            Assert.AreEqual(test.StartDate, ViewData.StartDate, "StartDate was not mapped.");
            Assert.AreEqual(test.EndDate, ViewData.EndDate, "EndDate was not mapped.");
            Assert.AreEqual(test.OriginalItemId, ViewData.OriginalItemId, "OriginalItemId was not mapped.");
            Assert.AreEqual(test.TestState, ViewData.testState, "TestState was not mapped.");
            Assert.AreEqual(test.Variants[0].VariantId, ViewData.VariantItemId, "Variants was not mapped.");
            Assert.AreEqual(test.MultivariateTestResults[0].Id, ViewData.TestResults[0].Id, "TestResults[0] was not mapped.");
            Assert.AreEqual(test.MultivariateTestResults[1].Id, ViewData.TestResults[1].Id, "TestResults[1] was not mapped.");
        }

        [TestMethod]
        public void CreateTestCallsTestRepoSave()
        {
            var repo = GetUnitUnderTest();

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestManager>()).Returns(_testmanager.Object);

            repo.CreateTest(viewdata);

            // Verify the testmanager was called by the repo with the proper argument
            _testmanager.Verify(tm => tm.Save(It.IsAny<MultivariateTest>()),
                Times.Once, "CreateTest did not call save");
        }

        [TestMethod]
        public void CreateTestCallsTestRepoSaveWithMappedData()
        {
            var repo = GetUnitUnderTest();

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestManager>()).Returns(_testmanager.Object);

            // now create the test and verify data was mapped properly
            repo.CreateTest(viewdata);

            // Verify the testmanager was called by the repo with the proper arguments - mapped
            _testmanager.Verify(tm => tm.Save(It.Is<MultivariateTest>(tc => tc.Id.Equals(viewdata.id))),
                Times.Once, "CreateTest did not call save with correctly mapped ID");
            _testmanager.Verify(tm => tm.Save(It.Is<MultivariateTest>(tc => tc.Title.Equals(viewdata.Title))),
                Times.Once, "CreateTest did not call save with correctly mapped Title");
            _testmanager.Verify(tm => tm.Save(It.Is<MultivariateTest>(tc => tc.Owner.Equals(viewdata.Owner))),
                Times.Once, "CreateTest did not call save with correctly mapped Owner");
            _testmanager.Verify(tm => tm.Save(It.Is<MultivariateTest>(tc => tc.StartDate.Equals(viewdata.StartDate))),
                Times.Once, "CreateTest did not call save with correctly mapped StartDate");
            _testmanager.Verify(tm => tm.Save(It.Is<MultivariateTest>(tc => tc.EndDate.Equals(viewdata.EndDate))),
                Times.Once, "CreateTest did not call save with correctly mapped EndDate");
            _testmanager.Verify(tm => tm.Save(It.Is<MultivariateTest>(tc => tc.OriginalItemId.Equals(viewdata.OriginalItemId))),
                Times.Once, "CreateTest did not call save with correctly mapped OriginalItemId");
            _testmanager.Verify(tm => tm.Save(It.Is<MultivariateTest>(tc => tc.Variants[0].VariantId.Equals(viewdata.VariantItemId))),
                Times.Once, "CreateTest did not call save with correctly mapped Variants[0].VariantId");
        }
    }
}
