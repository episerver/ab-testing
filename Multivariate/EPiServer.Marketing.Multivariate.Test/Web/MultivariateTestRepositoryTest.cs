using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using EPiServer.Marketing.Multivariate.Model;
using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Web.Models;

namespace EPiServer.Marketing.Multivariate.Test.Web
{
    [TestClass]
    public class MultivariateTestRepositoryTest
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IMultivariateTestManager> _testmanager;

        private MultivariateTestRepository GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _testmanager = new Mock<IMultivariateTestManager>();

            return new MultivariateTestRepository(_serviceLocator.Object);
        }

        [TestMethod]
        public void GetWinningTestResult_ReturnsCorrectResult()
        {
            Mock<IMultivariateTest> test = new Mock<IMultivariateTest>();
            var results = new List<MultivariateTestResult>();
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            results.Add(new MultivariateTestResult() { Views = 100, Conversions = 90 });
            results.Add(new MultivariateTestResult() { Views = 200, Conversions = 100 });
            results.Add(new MultivariateTestResult() { Id = theGuid, Views = 300, Conversions = 300 });
            results.Add(new MultivariateTestResult() { Views = 400, Conversions = 100 });
            results.Add(new MultivariateTestResult() { Views = 500, Conversions = 200 });
            test.Setup(t => t.MultivariateTestResults).Returns(results);

            MultivariateTestRepository repo = GetUnitUnderTest();

            var winner = repo.GetWinningTestResult(test.Object);

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
        public void GetTestByIdCallsTestManagerWithProperTestGuid()
        {
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            var repo = GetUnitUnderTest();

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestManager>()).Returns(_testmanager.Object);

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

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestManager>()).Returns(_testmanager.Object);

            repo.GetTestList(criteria);

            // Verify the testmanager was called by the repo with the proper argument
            _testmanager.Verify(tm => tm.GetTestList(It.Is<MultivariateTestCriteria>(tc => tc.Equals(criteria))),
                Times.Once, "GetTestList did not call TestManager.GetTestList or TestManager.GetTestList was not called with expected values");
        }

        [TestMethod]
        public void ConvertToViewModelConvertsProperly()
        {
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            Guid original= new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE7");
            Guid varient = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE6");
            Guid result1 = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE5");
            Guid result2 = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE4");

            MultivariateTest testToConvert = new MultivariateTest() {
                Id = theGuid, Title = "Title", Owner = "Owner",
                StartDate = DateTime.Today.AddDays(1), EndDate = DateTime.Today.AddDays(2),
                OriginalItemId = original,
                Variants = new List<Variant>() { new Variant() { Id = varient } },
                MultivariateTestResults = new List<MultivariateTestResult>() {
                    new MultivariateTestResult() { Id = result1 },
                    new MultivariateTestResult() { Id = result2 }
                }
            };

            var repo = GetUnitUnderTest();
            var ViewData = repo.ConvertToViewModel(testToConvert);

            // Verify the testmanager was called by the repo with the proper argument
            Assert.AreEqual(testToConvert.Id, ViewData.id, "Test ID was not mapped.");
            Assert.AreEqual(testToConvert.Title, ViewData.Title, "Title was not mapped.");
            Assert.AreEqual(testToConvert.Owner, ViewData.Owner, "Owner was not mapped.");
            Assert.AreEqual(testToConvert.StartDate, ViewData.StartDate, "StartDate was not mapped.");
            Assert.AreEqual(testToConvert.EndDate, ViewData.EndDate, "EndDate was not mapped.");
            Assert.AreEqual(testToConvert.OriginalItemId, ViewData.OriginalItemId, "OriginalItemId was not mapped.");
            Assert.AreEqual(testToConvert.Variants[0].VariantId, ViewData.VariantItemId, "Variants was not mapped.");
            Assert.AreEqual(testToConvert.MultivariateTestResults[0].Id, ViewData.TestResults[0].Id, "TestResults[0] was not mapped.");
            Assert.AreEqual(testToConvert.MultivariateTestResults[1].Id, ViewData.TestResults[1].Id, "TestResults[1] was not mapped.");
        }

        [TestMethod]
        public void CreateTestCallsTestRepoSave()
        {
            MultivariateTestViewModel viewdata = new MultivariateTestViewModel();
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
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            Guid original = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE7");
            Guid varient = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE6");
            Guid result1 = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE5");
            Guid result2 = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE4");
            MultivariateTestViewModel viewdata = new MultivariateTestViewModel()
            {
                id = theGuid,
                Title= "Title",
                Owner = Security.PrincipalInfo.CurrentPrincipal.Identity.Name, // Repo / business logic sets it to this.
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(2),
                OriginalItemId = original,
                VariantItemId = varient,
                TestResults = new List<MultivariateTestResult>() {
                    new MultivariateTestResult() { Id = result1 },
                    new MultivariateTestResult() { Id = result2 }
                }
            };
            var repo = GetUnitUnderTest();

            // Setup the service locator to return our mocked testmanager class
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestManager>()).Returns(_testmanager.Object);

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
