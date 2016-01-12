using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.Marketing.Multivariate.Test.Core;

namespace EPiServer.Marketing.Multivariate.Test.Dal
{
    [TestClass]
    public class MultiVariantDataAccessTests : TestBase
    {
        private TestContext _context;
        private DbConnection _dbConnection;
        private MultiVariantDataAccess _mtm;

        [TestInitialize]
        public void Initialization()
        {
            _dbConnection = Effort.DbConnectionFactory.CreateTransient();
            _context = new TestContext(_dbConnection);
            _mtm = new MultiVariantDataAccess(new Core.TestRepository(_context));
        }

        [TestMethod]
        public void MultivariateTestManagerGet()
        {
            var id = Guid.NewGuid();

            var test = new MultivariateTest()
            {
                Id = id,
                Title = "test",
                CreatedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                TestState = TestState.Active,
                Owner = "Bert"
            };

            _mtm.Save(test);

            Assert.AreEqual(_mtm.Get(id), test);
        }

        [TestMethod]
        public void MultivariateTestManagerGetTestList()
        {
            var tests = AddMultivariateTests(_mtm, 3);
            var list = _mtm.GetTestList(new MultivariateTestCriteria());
            Assert.AreEqual(list.Count(), 3);
        }

        [TestMethod]
        public void MultivariateTestManagerSave()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            var newTitle = "newTitle";
            tests[0].Title = newTitle;
            _mtm.Save(tests[0]);

            Assert.AreEqual(_mtm.Get(tests[0].Id).Title, newTitle);
        }

        [TestMethod]
        public void MultivariateTestManagerDelete()
        {
            var tests = AddMultivariateTests(_mtm, 3);

            _mtm.Delete(tests[0].Id);
            _mtm._repository.SaveChanges();

            Assert.AreEqual(_mtm._repository.GetAll().Count(), 2);
        }

        [TestMethod]
        public void MultivariateTestManagerStart()
        {
            var tests = AddMultivariateTests(_mtm, 1);

            _mtm.Start(tests[0].Id);

            Assert.AreEqual(_mtm.Get(tests[0].Id).TestState, TestState.Active);
        }

        [TestMethod]
        public void MultivariateTestManagerStop()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            tests[0].TestState = TestState.Active;
            _mtm.Save(tests[0]);

            _mtm.Stop(tests[0].Id);

            Assert.AreEqual(_mtm.Get(tests[0].Id).TestState, TestState.Done);
        }

        [TestMethod]
        public void MultivariateTestManagerArchive()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            tests[0].TestState = TestState.Active;
            _mtm.Save(tests[0]);

            _mtm.Archive(tests[0].Id);

            Assert.AreEqual(_mtm.Get(tests[0].Id).TestState, TestState.Archived);
        }

        [TestMethod]
        public void MultivariateTestManagerIncrementCount()
        {
            var testId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            var test = new MultivariateTest()
            {
                Id = testId,
                Title = "test",
                CreatedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                Owner = "Bert",
                MultivariateTestResults = new List<MultivariateTestResult>(),
                OriginalItemId = itemId
            };

            _mtm.Save(test);

            var result = new MultivariateTestResult()
            {
                ItemId = itemId,
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Views = 0,
                Conversions = 0
            };

            test.MultivariateTestResults.Add(result);

            _mtm.Save(test);

            // check that a result exists
            Assert.AreEqual(test.MultivariateTestResults.Count(), 1);

            _mtm.IncrementCount(testId, itemId, CountType.View);
            _mtm.IncrementCount(testId, itemId, CountType.Conversion);

            // check the result is incremented correctly
            Assert.AreEqual(test.MultivariateTestResults.FirstOrDefault(r => r.ItemId == itemId).Views, 1);
            Assert.AreEqual(test.MultivariateTestResults.FirstOrDefault(r => r.ItemId == itemId).Conversions, 1);
        }

        [TestMethod]
        public void MultivariateTestManagerAddNoId()
        {
            var test = new MultivariateTest()
            {
                Title = "test",
                CreatedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                TestState = TestState.Active,
                Owner = "Bert"
            };

            _mtm._repository.Add(test);
            _mtm._repository.SaveChanges();

            Assert.AreEqual(_mtm._repository.GetAll().Count(), 1);
        }
    }
}
