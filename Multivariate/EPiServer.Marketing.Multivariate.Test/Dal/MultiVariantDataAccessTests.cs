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
        public void MultivariateTestManagerGetTestListNoFilter()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
            var tests = AddMultivariateTests(_context, 2);

            var list = _mtm.GetTestList(new MultivariateTestCriteria());
            Assert.AreEqual(2, list.Count());
        }

        [TestMethod]
        public void MultivariateTestManagerGetTestListVariantFilter()
        {
            var variantItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
            var tests = AddMultivariateTests(_context, 3);
            tests[0].Variants.Add(new Variant() { Id = Guid.NewGuid(), VariantId = variantItemId });
            tests[1].Variants.Add(new Variant() { Id = Guid.NewGuid(), VariantId = variantItemId });
            _context.SaveChanges();

            var criteria = new MultivariateTestCriteria();
            var filter = new MultivariateTestFilter(MultivariateTestProperty.VariantId, FilterOperator.And, variantItemId);
            criteria.AddFilter(filter);
            var list = _mtm.GetTestList(criteria);
            Assert.AreEqual(2, list.Count());
        }


        [TestMethod]
        public void MultivariateTestManagerGetTestListOneFilter()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
            var tests = AddMultivariateTests(_context, 3);
            tests[0].OriginalItemId = Guid.NewGuid();
            tests[1].OriginalItemId = Guid.NewGuid();
            tests[2].OriginalItemId = originalItemId;
            _context.SaveChanges();

            var criteria = new MultivariateTestCriteria();
            var filter = new MultivariateTestFilter(MultivariateTestProperty.OriginalItemId, FilterOperator.And, originalItemId);
            criteria.AddFilter(filter);
            var list = _mtm.GetTestList(criteria);
            Assert.AreEqual(1, list.Count());
        }

        [TestMethod]
        public void MultivariateTestManagerGetTestListTwoFiltersAnd()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
           
            var tests = AddMultivariateTests(_context, 3);
            tests[0].OriginalItemId = originalItemId;
            tests[0].TestState = TestState.Archived;
            _context.SaveChanges();
            
            var criteria = new MultivariateTestCriteria();
            var filter = new MultivariateTestFilter(MultivariateTestProperty.OriginalItemId, FilterOperator.And, originalItemId);
            var filter2 = new MultivariateTestFilter(MultivariateTestProperty.TestState, FilterOperator.And, TestState.Archived);
            criteria.AddFilter(filter);
            criteria.AddFilter(filter2);
            var list = _mtm.GetTestList(criteria);
            Assert.AreEqual(1, list.Count());
        }

        [TestMethod]
        public void MultivariateTestManagerGetTestListTwoFiltersOr()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");

            var tests = AddMultivariateTests(_context, 3);
            tests[0].OriginalItemId = originalItemId;
            tests[1].TestState = TestState.Archived;
            tests[2].TestState = TestState.Archived;
            _context.SaveChanges();

            var criteria = new MultivariateTestCriteria();
            var filter = new MultivariateTestFilter(MultivariateTestProperty.OriginalItemId, FilterOperator.Or, originalItemId);
            var filter2 = new MultivariateTestFilter(MultivariateTestProperty.TestState, FilterOperator.Or, TestState.Archived);
            criteria.AddFilter(filter);
            criteria.AddFilter(filter2);
            var list = _mtm.GetTestList(criteria);
            Assert.AreEqual(3, list.Count());
        }

        [TestMethod]
        public void MultivariateTestManagerGetTestListTwoFiltersAndVariant()
        {
            var variantItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");

            var tests = AddMultivariateTests(_context, 3);
            tests[0].Variants.Add(new Variant() { Id = Guid.NewGuid(), VariantId = variantItemId });
            tests[1].TestState = TestState.Archived;
            _context.SaveChanges();

            var criteria = new MultivariateTestCriteria();
            var filter = new MultivariateTestFilter(MultivariateTestProperty.VariantId, FilterOperator.And, variantItemId);
            var filter2 = new MultivariateTestFilter(MultivariateTestProperty.TestState, FilterOperator.Or, TestState.Archived);
            criteria.AddFilter(filter);
            criteria.AddFilter(filter2);
            var list = _mtm.GetTestList(criteria);
            Assert.AreEqual(0, list.Count());
        }

        [TestMethod]
        public void MultivariateTestManagerGetTestListTwoFiltersOrVariant()
        {
            var variantItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");

            var tests = AddMultivariateTests(_context, 3);
            tests[0].Variants.Add(new Variant() { Id = Guid.NewGuid(), VariantId = variantItemId });
            tests[1].TestState = TestState.Active;
            _context.SaveChanges();
            
            var criteria = new MultivariateTestCriteria();
            var filter = new MultivariateTestFilter(MultivariateTestProperty.VariantId, FilterOperator.Or, variantItemId);
            var filter2 = new MultivariateTestFilter(MultivariateTestProperty.TestState, FilterOperator.And, TestState.Active);
            criteria.AddFilter(filter);
            criteria.AddFilter(filter2);


            var list = _mtm.GetTestList(criteria);
            Assert.AreEqual(2, list.Count());
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

        [TestMethod]
        public void MultivariteTestManagerMultivariateDataAccess()
        {
            MultiVariantDataAccess mda = new MultiVariantDataAccess();
            Assert.IsNotNull(mda._repository);
        }

        [TestMethod]
        public void MultivariteTestManagerGetTestByItemId()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
            var tests = AddMultivariateTests(_context, 2);
            tests[0].OriginalItemId = originalItemId;
            _mtm.Save(tests[0]);
            
            var list = _mtm.GetTestByItemId(originalItemId);
            Assert.AreEqual(1, list.Count());
        }
    }
}
