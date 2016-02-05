using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Model.Enums;
using EPiServer.Marketing.Multivariate.Test.Core;
using Xunit;

namespace EPiServer.Marketing.Multivariate.Test.Dal
{
    public class MultiVariantDataAccessTests : TestBase
    {
        private TestContext _context;
        private DbConnection _dbConnection;
        private TestingDataAccess _mtm;
        public MultiVariantDataAccessTests()
        {
            _dbConnection = Effort.DbConnectionFactory.CreateTransient();
            _context = new TestContext(_dbConnection);
            _mtm = new TestingDataAccess(new Core.TestRepository(_context));
        }

        [Fact]
        public void TestManagerGet()
        {
            var id = Guid.NewGuid();

            var test = new ABTest()
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

            Assert.Equal(_mtm.Get(id), test);
        }

        [Fact]
        public void TestManagerGetTestListNoFilter()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
            var tests = AddMultivariateTests(_context, 2);

            var list = _mtm.GetTestList(new TestCriteria());
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public void TestManagerGetTestListVariantFilter()
        {
            var variantItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
            var tests = AddMultivariateTests(_context, 3);
            tests[0].Variants.Add(new Variant() { Id = Guid.NewGuid(), VariantId = variantItemId });
            tests[1].Variants.Add(new Variant() { Id = Guid.NewGuid(), VariantId = variantItemId });
            _context.SaveChanges();

            var criteria = new TestCriteria();
            var filter = new MultivariateTestFilter(MultivariateTestProperty.VariantId, FilterOperator.And, variantItemId);
            criteria.AddFilter(filter);
            var list = _mtm.GetTestList(criteria);
            Assert.Equal(2, list.Count());
        }


        [Fact]
        public void TestManagerGetTestListOneFilter()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
            var tests = AddMultivariateTests(_context, 3);
            tests[0].OriginalItemId = Guid.NewGuid();
            tests[1].OriginalItemId = Guid.NewGuid();
            tests[2].OriginalItemId = originalItemId;
            _context.SaveChanges();

            var criteria = new TestCriteria();
            var filter = new MultivariateTestFilter(MultivariateTestProperty.OriginalItemId, FilterOperator.And, originalItemId);
            criteria.AddFilter(filter);
            var list = _mtm.GetTestList(criteria);
            Assert.Equal(1, list.Count());
        }

        [Fact]
        public void TestManagerGetTestListTwoFiltersAnd()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");

            var tests = AddMultivariateTests(_context, 3);
            tests[0].OriginalItemId = originalItemId;
            tests[0].TestState = TestState.Archived;
            _context.SaveChanges();

            var criteria = new TestCriteria();
            var filter = new MultivariateTestFilter(MultivariateTestProperty.OriginalItemId, FilterOperator.And, originalItemId);
            var filter2 = new MultivariateTestFilter(MultivariateTestProperty.TestState, FilterOperator.And, TestState.Archived);
            criteria.AddFilter(filter);
            criteria.AddFilter(filter2);
            var list = _mtm.GetTestList(criteria);
            Assert.Equal(1, list.Count());
        }

        [Fact]
        public void TestManagerGetTestListTwoFiltersOr()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");

            var tests = AddMultivariateTests(_context, 3);
            tests[0].OriginalItemId = originalItemId;
            tests[1].TestState = TestState.Archived;
            tests[2].TestState = TestState.Archived;
            _context.SaveChanges();

            var criteria = new TestCriteria();
            var filter = new MultivariateTestFilter(MultivariateTestProperty.OriginalItemId, FilterOperator.Or, originalItemId);
            var filter2 = new MultivariateTestFilter(MultivariateTestProperty.TestState, FilterOperator.Or, TestState.Archived);
            criteria.AddFilter(filter);
            criteria.AddFilter(filter2);
            var list = _mtm.GetTestList(criteria);
            Assert.Equal(3, list.Count());
        }

        [Fact]
        public void TestManagerGetTestListTwoFiltersAndVariant()
        {
            var variantItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");

            var tests = AddMultivariateTests(_context, 3);
            tests[0].Variants.Add(new Variant() { Id = Guid.NewGuid(), VariantId = variantItemId });
            tests[1].TestState = TestState.Archived;
            _context.SaveChanges();

            var criteria = new TestCriteria();
            var filter = new MultivariateTestFilter(MultivariateTestProperty.VariantId, FilterOperator.And, variantItemId);
            var filter2 = new MultivariateTestFilter(MultivariateTestProperty.TestState, FilterOperator.Or, TestState.Archived);
            criteria.AddFilter(filter);
            criteria.AddFilter(filter2);
            var list = _mtm.GetTestList(criteria);
            Assert.Equal(0, list.Count());
        }

        [Fact]
        public void TestManagerGetTestListTwoFiltersOrVariant()
        {
            var variantItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");

            var tests = AddMultivariateTests(_context, 3);
            tests[0].Variants.Add(new Variant() { Id = Guid.NewGuid(), VariantId = variantItemId });
            tests[1].TestState = TestState.Active;
            _context.SaveChanges();

            var criteria = new TestCriteria();
            var filter = new MultivariateTestFilter(MultivariateTestProperty.VariantId, FilterOperator.Or, variantItemId);
            var filter2 = new MultivariateTestFilter(MultivariateTestProperty.TestState, FilterOperator.And, TestState.Active);
            criteria.AddFilter(filter);
            criteria.AddFilter(filter2);


            var list = _mtm.GetTestList(criteria);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public void TestManagerSave()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            var newTitle = "newTitle";
            tests[0].Title = newTitle;
            _mtm.Save(tests[0]);

            Assert.Equal(_mtm.Get(tests[0].Id).Title, newTitle);
        }

        [Fact]
        public void TestManagerDelete()
        {
            var tests = AddMultivariateTests(_mtm, 3);

            _mtm.Delete(tests[0].Id);
            _mtm._repository.SaveChanges();

            Assert.Equal(_mtm._repository.GetAll().Count(), 2);
        }

        [Fact]
        public void TestManagerStart()
        {
            var tests = AddMultivariateTests(_mtm, 1);

            _mtm.Start(tests[0].Id);

            Assert.Equal(_mtm.Get(tests[0].Id).TestState, TestState.Active);
        }

        [Fact]
        public void TestManagerStop()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            tests[0].TestState = TestState.Active;
            _mtm.Save(tests[0]);

            _mtm.Stop(tests[0].Id);

            Assert.Equal(_mtm.Get(tests[0].Id).TestState, TestState.Done);
        }

        [Fact]
        public void TestManagerArchive()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            tests[0].TestState = TestState.Active;
            _mtm.Save(tests[0]);

            _mtm.Archive(tests[0].Id);

            Assert.Equal(_mtm.Get(tests[0].Id).TestState, TestState.Archived);
        }

        [Fact]
        public void TestManagerIncrementCount()
        {
            var testId = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            var test = new ABTest()
            {
                Id = testId,
                Title = "test",
                CreatedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                Owner = "Bert",
                MultivariateTestResults = new List<TestResult>(),
                Variants = new List<Variant>(),
                OriginalItemId = itemId
            };

            _mtm.Save(test);

            var result = new TestResult()
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
            Assert.Equal(test.MultivariateTestResults.Count(), 1);

            _mtm.IncrementCount(testId, itemId, CountType.View);
            _mtm.IncrementCount(testId, itemId, CountType.Conversion);

            // check the result is incremented correctly
            Assert.Equal(test.MultivariateTestResults.FirstOrDefault(r => r.ItemId == itemId).Views, 1);
            Assert.Equal(test.MultivariateTestResults.FirstOrDefault(r => r.ItemId == itemId).Conversions, 1);
        }

        [Fact]
        public void TestManagerAddNoId()
        {
            var test = new ABTest()
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

            Assert.Equal(_mtm._repository.GetAll().Count(), 1);
        }

        [Fact]
        public void MultivariteTestManagerMultivariateDataAccess()
        {
            TestingDataAccess mda = new TestingDataAccess();
            Assert.NotNull(mda._repository);
        }

        [Fact]
        public void MultivariteTestManagerGetTestByItemId()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
            var tests = AddMultivariateTests(_context, 2);
            tests[0].OriginalItemId = originalItemId;
            _mtm.Save(tests[0]);

            var list = _mtm.GetTestByItemId(originalItemId);
            Assert.Equal(1, list.Count());
        }
    }
}
