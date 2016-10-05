using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Dal.DataAccess;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using Xunit;
using EPiServer.ServiceLocation;
using Moq;

namespace EPiServer.Marketing.Testing.Test.Dal
{
    public class MultiVariantDataAccessTests : TestBase
    {
        private TestContext _context;
        private DbConnection _dbConnection;
        private TestingDataAccess _mtm;
        private TestManager _tm;
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IKpiManager> _kpiManager;
        private DefaultMarketingTestingEvents _marketingEvents;

        public MultiVariantDataAccessTests()
        {
            _dbConnection = Effort.DbConnectionFactory.CreateTransient();
            _context = new TestContext(_dbConnection);
            _mtm = new TestingDataAccess(new Core.TestRepository(_context));
            _marketingEvents = new DefaultMarketingTestingEvents();

            _kpiManager = new Mock<IKpiManager>();
            _kpiManager.Setup(call => call.Save(It.IsAny<IKpi>())).Returns(It.IsAny<Guid>());

            _serviceLocator = new Mock<IServiceLocator>();
            _serviceLocator.Setup(sl => sl.GetInstance<ITestingDataAccess>()).Returns(_mtm);
            _serviceLocator.Setup(sl => sl.GetInstance<IKpiManager>()).Returns(_kpiManager.Object);
            _serviceLocator.Setup(sl => sl.GetInstance<DefaultMarketingTestingEvents>()).Returns(_marketingEvents);

            _tm = new TestManager(_serviceLocator.Object);
        }

        [Fact]
        public void TestManagerGet()
        {
            var id = Guid.NewGuid();

            var test = new DalABTest()
            {
                Id = id,
                Title = "test",
                Description = "description",
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                State = DalTestState.Active,
                ConfidenceLevel = 95,
                Owner = "Bert",
                KeyPerformanceIndicators = new List<DalKeyPerformanceIndicator>(),
                Variants = new List<DalVariant>()
            };

            _mtm.Save(test);

            Assert.Equal(_mtm.Get(id), test);
        }

        [Fact]
        public void TestManagerGetTestListNoFilter()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
            var tests = AddMultivariateTests(_context, 2);

            var list = _mtm.GetTestList(new DalTestCriteria());
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public void TestManagerGetTestListVariantFilter()
        {
            var variantItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
            var tests = AddMultivariateTests(_context, 3);
            tests[0].Variants.Add(new DalVariant() { Id = Guid.NewGuid(), ItemId = variantItemId,
                DalKeyValueResults = new List<DalKeyValueResult>()
                {
                    new DalKeyValueResult() { KpiId = Guid.NewGuid(), Value = 12, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow, Id = Guid.NewGuid()}
                }
            });
            tests[1].Variants.Add(new DalVariant() { Id = Guid.NewGuid(), ItemId = variantItemId,
                DalKeyFinancialResults = new List<DalKeyFinancialResult>()
                {
                    new DalKeyFinancialResult() { KpiId = Guid.NewGuid(), Total = 12, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow, Id = Guid.NewGuid()}
                }
            });
            _context.SaveChanges();

            var criteria = new DalTestCriteria();
            var filter = new DalABTestFilter(DalABTestProperty.VariantId, DalFilterOperator.And, variantItemId);
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

            var criteria = new DalTestCriteria();
            var filter = new DalABTestFilter();
            filter.Property = DalABTestProperty.OriginalItemId;
            filter.Operator = DalFilterOperator.And;
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
            tests[0].State = DalTestState.Archived;
            _context.SaveChanges();

            var criteria = new DalTestCriteria();
            var filter = new DalABTestFilter(DalABTestProperty.OriginalItemId, DalFilterOperator.And, originalItemId);
            var filter2 = new DalABTestFilter(DalABTestProperty.State, DalFilterOperator.And, DalTestState.Archived);
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
            tests[1].State = DalTestState.Archived;
            tests[2].State = DalTestState.Archived;
            _context.SaveChanges();

            var criteria = new DalTestCriteria();
            var filter = new DalABTestFilter(DalABTestProperty.OriginalItemId, DalFilterOperator.Or, originalItemId);
            var filter2 = new DalABTestFilter(DalABTestProperty.State, DalFilterOperator.Or, DalTestState.Archived);
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
            tests[0].Variants.Add(new DalVariant() { Id = Guid.NewGuid(), ItemId = variantItemId });
            tests[1].State = DalTestState.Archived;
            _context.SaveChanges();

            var criteria = new DalTestCriteria();
            var filter = new DalABTestFilter(DalABTestProperty.VariantId, DalFilterOperator.And, variantItemId);
            var filter2 = new DalABTestFilter(DalABTestProperty.State, DalFilterOperator.Or, DalTestState.Archived);
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
            tests[0].Variants.Add(new DalVariant() { Id = Guid.NewGuid(), ItemId = variantItemId });
            tests[1].State = DalTestState.Active;
            _context.SaveChanges();

            var criteria = new DalTestCriteria();
            var filter = new DalABTestFilter(DalABTestProperty.VariantId, DalFilterOperator.Or, variantItemId);
            var filter2 = new DalABTestFilter(DalABTestProperty.State, DalFilterOperator.And, DalTestState.Active);
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

            Assert.Equal(_mtm.Get(tests[0].Id).State, DalTestState.Active);
        }

        [Fact]
        public void TestManagerStop()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            tests[0].State = DalTestState.Active;
            _mtm.Save(tests[0]);

            _mtm.Stop(tests[0].Id);

            Assert.Equal(_mtm.Get(tests[0].Id).State, DalTestState.Done);
        }

        [Fact]
        public void TestManagerArchive()
        {
            var testId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var itemVersion = 1;

            var test = new DalABTest()
            {
                Id = testId,
                State = DalTestState.Active,
                Title = "test",
                CreatedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                Owner = "Bert",
                Variants = new List<DalVariant>(),
                KeyPerformanceIndicators = new List<DalKeyPerformanceIndicator>(),
                ParticipationPercentage = 100,
                OriginalItemId = itemId
            };

            //_mtm.Save(test);

            var variant = new DalVariant()
            {
                ItemId = itemId,
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Views = 0,
                Conversions = 0,
                ItemVersion = itemVersion
            };

            test.Variants.Add(variant);
            var retTestid = _mtm.Save(test);

            _mtm.Archive(retTestid,variant.Id);

            var retTest = _mtm.Get(retTestid);
            // check that a variant is set to winner
            Assert.Equal(retTest.Variants[0].IsWinner,true);
            
            // check the test is archived
            Assert.True(retTest.State == DalTestState.Archived);
            
        }

        [Fact]
        public void TestManagerIncrementCount()
        {
            var testId = Guid.NewGuid();
            var itemId = Guid.NewGuid();
            var itemVersion = 1;

            var test = new DalABTest()
            {
                Id = testId,
                Title = "test",
                CreatedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                Owner = "Bert",
                Variants = new List<DalVariant>(),
                KeyPerformanceIndicators = new List<DalKeyPerformanceIndicator>(),
                ParticipationPercentage = 100,
                OriginalItemId = itemId
            };

            //_mtm.Save(test);

            var variant = new DalVariant()
            {
                ItemId = itemId,
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Views = 0,
                Conversions = 0,
                ItemVersion = itemVersion
            };

            test.Variants.Add(variant);

            _mtm.Save(test);

            // check that a variant exists
            Assert.Equal(test.Variants.Count(), 1);

            _mtm.IncrementCount(testId, itemId, itemVersion, DalCountType.View);
            _mtm.IncrementCount(testId, itemId, itemVersion, DalCountType.Conversion);

            // check the variant is incremented correctly
            Assert.Equal(1, test.Variants.FirstOrDefault(r => r.ItemId == itemId && r.ItemVersion == itemVersion).Views);
            Assert.Equal(1, test.Variants.FirstOrDefault(r => r.ItemId == itemId && r.ItemVersion == itemVersion).Conversions);
        }

        [Fact]
        public void TestManagerAddNoId()
        {
            var test = new DalABTest()
            {
                Title = "test",
                Description = "Description",
                CreatedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                State = DalTestState.Active,
                Owner = "Bert"
            };

            _mtm._repository.Add(test);
            _mtm._repository.SaveChanges();

            Assert.Equal(1, _mtm._repository.GetAll().Count());
        }

        [Fact]
        public void MultivariteTestManagerMultivariateDataAccess()
        {
            TestingDataAccess mda = new TestingDataAccess();
            Assert.True(mda._UseEntityFramework);
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

            var variant = new DalVariant() {Id = Guid.NewGuid(), ItemId = originalItemId, ItemVersion = 1, TestId = tests[0].Id };
            tests[0].Variants.Add(variant);

            var variant2 = new DalVariant() {Id = Guid.NewGuid(), ItemId = originalItemId, ItemVersion = 1, TestId = tests[0].Id };
            tests[0].Variants.Add(variant2);
            _mtm.Save(tests[0]);

            variant.ItemVersion = 2;
            _mtm.Save(tests[0]);

            Assert.Equal(originalItemId, _mtm.Get(tests[0].Id).OriginalItemId);
            Assert.Equal(2, _mtm.Get(tests[0].Id).Variants.First(v => v.ItemId == originalItemId).ItemVersion);

        }

        [Fact]
        public void TestManagerSaveAddVariantItem()
        {
            var tests = AddMultivariateTests(_mtm, 1);
            var originalItemId = Guid.NewGuid();
            tests[0].OriginalItemId = originalItemId;
            var variant = new DalVariant() { Id = Guid.NewGuid(), ItemId = originalItemId, ItemVersion = 1 };
            tests[0].Variants.Add(variant);

            _mtm.Save(tests[0]);

            var variantItemId2 = Guid.NewGuid();
            var variant2 = new DalVariant() { Id = Guid.NewGuid(), ItemId = variantItemId2, ItemVersion = 1 };
            tests[0].Variants.Add(variant2);

            _mtm.Save(tests[0]);

            Assert.Equal(originalItemId, _mtm.Get(tests[0].Id).OriginalItemId);
            Assert.Equal(2, _mtm.Get(tests[0].Id).Variants.Count);
        }

        [Fact]
        public void TestManagerSaveDone()
        {
            //var tm = GetUnitUnderTest();

            var id = Guid.NewGuid();
            var itemId = Guid.NewGuid();

            var test = new ABTest()
            {
                Id = id,
                State = TestState.Done,
                Variants =
                    new List<Variant>()
                    {
                        new Variant() {Id = Guid.NewGuid(), ItemVersion = 1, ItemId = itemId, Views = 5000, Conversions = 130, KeyFinancialResults = new List<KeyFinancialResult>(),
                            KeyValueResults = new List<KeyValueResult>() },
                        new Variant() {Id = Guid.NewGuid(), ItemVersion = 1, ItemId = itemId, Views = 5000, Conversions = 100, KeyFinancialResults = new List<KeyFinancialResult>(),
                            KeyValueResults = new List<KeyValueResult>() }
                    },
                KpiInstances = new List<IKpi>() { new ContentComparatorKPI() { Id = Guid.NewGuid(), ContentGuid = Guid.NewGuid() } },
                Title = "test",
                Description = "description",
                CreatedDate = DateTime.UtcNow,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                ParticipationPercentage = 100,
                LastModifiedBy = "me",
                OriginalItemId = Guid.NewGuid(),
                Owner = "Bert",
                ZScore = 0,
                IsSignificant = false

            };


            _tm.Save(test);

            //var testt = tm.Get(id);

            test.State = TestState.Archived;
            test.ZScore = 2.0;
            test.IsSignificant = true;

            Assert.Equal(id, _tm.Save(test));
        }

    }
}
