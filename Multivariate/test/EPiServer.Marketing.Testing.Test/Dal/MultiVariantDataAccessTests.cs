using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.Entity;
using EPiServer.Marketing.Testing.Dal.Entity.Enums;
using EPiServer.Marketing.Testing.Tests;
using Xunit;

namespace EPiServer.Marketing.Testing.Tests.Dal
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
                Description = "description",
                CreatedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                State = TestState.Active,
                Owner = "Bert",
                KeyPerformanceIndicators = new List<KeyPerformanceIndicator>(),
                TestResults = new List<TestResult>(),
                Variants = new List<Variant>()
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
            tests[0].Variants.Add(new Variant() { Id = Guid.NewGuid(), ItemId = variantItemId });
            tests[1].Variants.Add(new Variant() { Id = Guid.NewGuid(), ItemId = variantItemId });
            _context.SaveChanges();

            var criteria = new TestCriteria();
            var filter = new ABTestFilter(ABTestProperty.VariantId, FilterOperator.And, variantItemId);
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
            var filter = new ABTestFilter();
            filter.Property = ABTestProperty.OriginalItemId;
            filter.Operator = FilterOperator.And;
            filter.Value = originalItemId;
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
            tests[0].State = TestState.Archived;
            _context.SaveChanges();

            var criteria = new TestCriteria();
            var filter = new ABTestFilter(ABTestProperty.OriginalItemId, FilterOperator.And, originalItemId);
            var filter2 = new ABTestFilter(ABTestProperty.State, FilterOperator.And, TestState.Archived);
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
            tests[1].State = TestState.Archived;
            tests[2].State = TestState.Archived;
            _context.SaveChanges();

            var criteria = new TestCriteria();
            var filter = new ABTestFilter(ABTestProperty.OriginalItemId, FilterOperator.Or, originalItemId);
            var filter2 = new ABTestFilter(ABTestProperty.State, FilterOperator.Or, TestState.Archived);
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
            tests[0].Variants.Add(new Variant() { Id = Guid.NewGuid(), ItemId = variantItemId });
            tests[1].State = TestState.Archived;
            _context.SaveChanges();

            var criteria = new TestCriteria();
            var filter = new ABTestFilter(ABTestProperty.VariantId, FilterOperator.And, variantItemId);
            var filter2 = new ABTestFilter(ABTestProperty.State, FilterOperator.Or, TestState.Archived);
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
            tests[0].Variants.Add(new Variant() { Id = Guid.NewGuid(), ItemId = variantItemId });
            tests[1].State = TestState.Active;
            _context.SaveChanges();

            var criteria = new TestCriteria();
            var filter = new ABTestFilter(ABTestProperty.VariantId, FilterOperator.Or, variantItemId);
            var filter2 = new ABTestFilter(ABTestProperty.State, FilterOperator.And, TestState.Active);
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
            var newDescription = "newDescription";
            tests[0].Description = newDescription;
            _mtm.Save(tests[0]);

            Assert.Equal(_mtm.Get(tests[0].Id).Title, newTitle);
            Assert.Equal(_mtm.Get(tests[0].Id).Description, newDescription);
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

            Assert.Equal(_mtm.Get(tests[0].Id).State, TestState.Active);
        }

        [Fact]
        public void TestManagerStop()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            tests[0].State = TestState.Active;
            _mtm.Save(tests[0]);

            _mtm.Stop(tests[0].Id);

            Assert.Equal(_mtm.Get(tests[0].Id).State, TestState.Done);
        }

        [Fact]
        public void TestManagerArchive()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            tests[0].State = TestState.Active;
            _mtm.Save(tests[0]);

            _mtm.Archive(tests[0].Id);

            Assert.Equal(_mtm.Get(tests[0].Id).State, TestState.Archived);
        }

        [Fact]
        public void TestManagerIncrementCount()
        {
            var testId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var itemVersion = 1;

            var test = new ABTest()
            {
                Id = testId,
                Title = "test",
                CreatedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                Owner = "Bert",
                TestResults = new List<TestResult>(),
                Variants = new List<Variant>(),
                KeyPerformanceIndicators = new List<KeyPerformanceIndicator>(),
                ParticipationPercentage = 100,
                OriginalItemId = itemId
            };

            //_mtm.Save(test);

            var result = new TestResult()
            {
                ItemId = itemId,
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Views = 0,
                Conversions = 0,
                ItemVersion = itemVersion
            };

            test.TestResults.Add(result);

            _mtm.Save(test);

            // check that a result exists
            Assert.Equal(test.TestResults.Count(), 1);

            _mtm.IncrementCount(testId, itemId, itemVersion, CountType.View);
            _mtm.IncrementCount(testId, itemId, itemVersion, CountType.Conversion);

            // check the result is incremented correctly
            Assert.Equal(test.TestResults.FirstOrDefault(r => r.ItemId == itemId && r.ItemVersion == itemVersion).Views, 1);
            Assert.Equal(test.TestResults.FirstOrDefault(r => r.ItemId == itemId && r.ItemVersion == itemVersion).Conversions, 1);
        }

        [Fact]
        public void TestManagerAddNoId()
        {
            var test = new ABTest()
            {
                Title = "test",
                Description = "Description",
                CreatedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                State = TestState.Active,
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

        [Fact]
        public void TestManagerSaveVariantUpdate()
        {
            var tests = AddMultivariateTests(_mtm, 1);

            var originalItemId = Guid.NewGuid();

            tests[0].OriginalItemId = originalItemId;

            var variant = new Variant() {Id = Guid.NewGuid(), ItemId = originalItemId, ItemVersion = 1, TestId = tests[0].Id };
            tests[0].Variants.Add(variant);

            var result = new TestResult() {Id = Guid.NewGuid(), ItemId = originalItemId, ItemVersion = 1, TestId = tests[0].Id };
            tests[0].TestResults.Add(result);
            _mtm.Save(tests[0]);

            variant.ItemVersion = 2;
            _mtm.Save(tests[0]);

            Assert.Equal(_mtm.Get(tests[0].Id).OriginalItemId, originalItemId);
            Assert.Equal(_mtm.Get(tests[0].Id).Variants.First(v => v.ItemId == originalItemId).ItemVersion, 2);

        }

        [Fact]
        public void TestManagerSaveAddVariantItem()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            var originalItemId = Guid.NewGuid();
            tests[0].OriginalItemId = originalItemId;
            var variant = new Variant() { Id = Guid.NewGuid(), ItemId = originalItemId, ItemVersion = 1 };
            tests[0].Variants.Add(variant);

            var result = new TestResult() { Id = Guid.NewGuid(), ItemId = originalItemId, ItemVersion = 1 };
            tests[0].TestResults.Add(result);
            _mtm.Save(tests[0]);

            var variantItemId2 = Guid.NewGuid();
            var variant2 = new Variant() { Id = Guid.NewGuid(), ItemId = variantItemId2, ItemVersion = 1 };
            tests[0].Variants.Add(variant2);

            var result2 = new TestResult() { Id = Guid.NewGuid(), ItemId = variantItemId2, ItemVersion = 1 };
            tests[0].TestResults.Add(result2);

            _mtm.Save(tests[0]);

            Assert.Equal(_mtm.Get(tests[0].Id).OriginalItemId, originalItemId);
            Assert.Equal(_mtm.Get(tests[0].Id).Variants.Count, 2);
        }

    }
}
