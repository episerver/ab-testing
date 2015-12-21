using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using EPiServer.Enterprise;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NMemory.Linq;
using TestContext = EPiServer.Marketing.Multivariate.Test.Dal.TestContext;


namespace EPiServer.Marketing.Multivariate.Test.Core
{
    [TestClass]
    public class MultivariateManagerTests : TestBase
    {
        private TestContext _context;
        private DbConnection _dbConnection;
        private MultivariateTestManager _mtm;

        [TestInitialize]
        public void Initialization()
        {
            _dbConnection = Effort.DbConnectionFactory.CreateTransient();
            _context = new TestContext(_dbConnection);
            _mtm = new MultivariateTestManager(new CurrentUser(), new CurrentSite(), new TestRepository(_context));
        }

        [TestMethod]
        public void TestManager_Construction_Creates_Internal_Objects()
        {
            var aTestManager = new MultivariateTestManager();

            //TODO: logger does not exist currently
            //Assert.IsNotNull(aTestManager._log, "The logger should be created upon construction");
            Assert.IsNotNull(aTestManager._repository, "The data access object should be created upon construction");
            Assert.IsNotNull(aTestManager._user, "The current user object should be created upon construction");
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
                TestState = (int)TestState.Active,
                Owner = "Bert"
            };

            _mtm.Save(test);

            Assert.AreEqual(_mtm.Get(id), test);
        }

       [TestMethod]
        public void MultivariateTestManagerGetTestList()
        {
            var tests = AddMultivariateTests(_mtm, 3);

            Assert.AreEqual(_mtm.GetTestList(new MultivariateTestCriteria()).Count(), 3);
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

            Assert.AreEqual(_mtm.Get(tests[0].Id).TestState, (int)TestState.Active);
        }

        [TestMethod]
        public void MultivariateTestManagerStop()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            tests[0].TestState = (int) TestState.Active;
            _mtm.Save(tests[0]);

            _mtm.Stop(tests[0].Id);

            Assert.AreEqual(_mtm.Get(tests[0].Id).TestState, (int)TestState.Done);
        }

        [TestMethod]
        public void MultivariateTestManagerArchive()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            tests[0].TestState = (int)TestState.Active;
            _mtm.Save(tests[0]);

            _mtm.Archive(tests[0].Id);

            Assert.AreEqual(_mtm.Get(tests[0].Id).TestState, (int)TestState.Archived);
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
        public void MultivariateTestManagerReturnLandingPage()
        {
            var itemId = Guid.NewGuid();
            var variantId = Guid.NewGuid();
            var tests = AddMultivariateTests(_mtm, 1);
            var test = tests[0];

            test.OriginalItemId = itemId;


            var variant = new Variant()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                VariantId = variantId
            };

            test.Variants.Add(variant);

            _mtm.Save(test);

            var landingPageId = _mtm.ReturnLandingPage(test.Id);

            Assert.IsTrue(landingPageId == itemId || landingPageId == variantId);
        }

        [TestMethod]
        public void MultivariateTestManagerAddNoId()
        {
            var test = new MultivariateTest()
            {
                Title = "test" ,
                CreatedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                TestState = (int)TestState.Active,
                Owner = "Bert"
            };

            _mtm._repository.Add(test);
            _mtm._repository.SaveChanges();

            Assert.AreEqual(_mtm._repository.GetAll().Count(), 1);
        }
       
    }
}