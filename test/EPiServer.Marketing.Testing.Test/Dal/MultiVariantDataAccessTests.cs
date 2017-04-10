using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Migrations.History;
using System.Linq;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Dal.DataAccess;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;
using Xunit;
using EPiServer.ServiceLocation;
using Moq;

namespace EPiServer.Marketing.Testing.Test.Dal
{
    public class MultiVariantDataAccessTests : TestBase
    {
        private TestContext _context;
        private HistoryContext _historyContext;
        private DbConnection _dbConnection;
        private TestingDataAccess _dataAccess;
        private TestingDataAccess _historyDataAccess;
        private TestManager _tm;
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IKpiManager> _kpiManager;
        private DefaultMarketingTestingEvents _marketingEvents;

        public MultiVariantDataAccessTests()
        {
            _dbConnection = Effort.DbConnectionFactory.CreateTransient();
            _context = new TestContext(_dbConnection);

            _dataAccess = new TestingDataAccess(new Core.TestRepository(_context));

            _marketingEvents = new DefaultMarketingTestingEvents();

            _kpiManager = new Mock<IKpiManager>();
            _kpiManager.Setup(call => call.Save(It.IsAny<List<IKpi>>())).Returns(It.IsAny<List<Guid>>());

             _serviceLocator = new Mock<IServiceLocator>();
             _serviceLocator.Setup(sl => sl.GetInstance<ITestingDataAccess>()).Returns(_dataAccess);
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

            _dataAccess.Save(test);

            Assert.Equal(_dataAccess.Get(id), test);
        }

        [Fact]
        public void TestManagerGetTestListNoFilter()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
            var tests = AddMultivariateTests(_context, 2);

            var list = _dataAccess.GetTestList(new DalTestCriteria());
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
                    new DalKeyFinancialResult() { KpiId = Guid.NewGuid(),
                        Total = 12,
                        CreatedDate = DateTime.UtcNow,
                        ModifiedDate = DateTime.UtcNow,
                        Id = Guid.NewGuid(),
                     ConvertedTotal = 400,
                     ConvertedTotalCulture = "SEK",
                     TotalMarketCulture = "en-US"
                    }
                }
            });
            _context.SaveChanges();

            var criteria = new DalTestCriteria();
            var filter = new DalABTestFilter(DalABTestProperty.VariantId, DalFilterOperator.And, variantItemId);
            criteria.AddFilter(filter);
            var list = _dataAccess.GetTestList(criteria);
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
            var list = _dataAccess.GetTestList(criteria);
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
            var list = _dataAccess.GetTestList(criteria);
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
            var list = _dataAccess.GetTestList(criteria);
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
            var list = _dataAccess.GetTestList(criteria);
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


            var list = _dataAccess.GetTestList(criteria);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public void TestManagerSave()
        {
            var tests = AddMultivariateTests(_dataAccess, 1);
            var newTitle = "newTitle";
            tests[0].Title = newTitle;
            var newDescription = "newDescription";
            tests[0].Description = newDescription;
            _dataAccess.Save(tests[0]);

            Assert.Equal(_dataAccess.Get(tests[0].Id).Title, newTitle);
            Assert.Equal(_dataAccess.Get(tests[0].Id).Description, newDescription);
        }

        [Fact]
        public void TestManagerDelete()
        {
            var tests = AddMultivariateTests(_dataAccess, 3);

            _dataAccess.Delete(tests[0].Id);
            _dataAccess._repository.SaveChanges();

            Assert.Equal(_dataAccess._repository.GetAll().Count(), 2);
        }

        [Fact]
        public void TestManagerStart()
        {
            var tests = AddMultivariateTests(_dataAccess, 1);

            _dataAccess.Start(tests[0].Id);

            Assert.Equal(_dataAccess.Get(tests[0].Id).State, DalTestState.Active);
        }

        [Fact]
        public void TestManagerStop()
        {
            var tests = AddMultivariateTests(_dataAccess, 1);
            tests[0].State = DalTestState.Active;
            _dataAccess.Save(tests[0]);

            _dataAccess.Stop(tests[0].Id);

            Assert.Equal(_dataAccess.Get(tests[0].Id).State, DalTestState.Done);
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
            var retTestid = _dataAccess.Save(test);

            _dataAccess.Archive(retTestid,variant.Id);

            var retTest = _dataAccess.Get(retTestid);
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

            _dataAccess.Save(test);

            // check that a variant exists
            Assert.Equal(test.Variants.Count(), 1);

            _dataAccess.IncrementCount(testId, itemVersion, DalCountType.View, new Guid());
            _dataAccess.IncrementCount(testId, itemVersion, DalCountType.Conversion, new Guid());

            // check the variant is incremented correctly
            Assert.Equal(1, test.Variants.FirstOrDefault(r => r.ItemVersion == itemVersion).Views);
            Assert.Equal(1, test.Variants.FirstOrDefault(r => r.ItemVersion == itemVersion).Conversions);
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

            _dataAccess._repository.Add(test);
            _dataAccess._repository.SaveChanges();

            Assert.Equal(1, _dataAccess._repository.GetAll().Count());
        }

        [Fact]
        public void MultivariteTestManagerGetTestByItemId()
        {
            var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
            var tests = AddMultivariateTests(_context, 2);
            tests[0].OriginalItemId = originalItemId;
            _dataAccess.Save(tests[0]);

            var list = _dataAccess.GetTestByItemId(originalItemId);
            Assert.Equal(1, list.Count());
        }

        [Fact]
        public void TestManagerSaveVariantUpdate()
        {
            var tests = AddMultivariateTests(_dataAccess, 1);

            var originalItemId = Guid.NewGuid();

            tests[0].OriginalItemId = originalItemId;

            var variant = new DalVariant() {Id = Guid.NewGuid(), ItemId = originalItemId, ItemVersion = 1, TestId = tests[0].Id };
            tests[0].Variants.Add(variant);

            var variant2 = new DalVariant() {Id = Guid.NewGuid(), ItemId = originalItemId, ItemVersion = 1, TestId = tests[0].Id };
            tests[0].Variants.Add(variant2);
            _dataAccess.Save(tests[0]);

            variant.ItemVersion = 2;
            _dataAccess.Save(tests[0]);

            Assert.Equal(originalItemId, _dataAccess.Get(tests[0].Id).OriginalItemId);
            Assert.Equal(2, _dataAccess.Get(tests[0].Id).Variants.First(v => v.ItemId == originalItemId).ItemVersion);

        }

        [Fact]
        public void TestManagerSaveAddVariantItem()
        {
            var tests = AddMultivariateTests(_dataAccess, 1);
            var originalItemId = Guid.NewGuid();
            tests[0].OriginalItemId = originalItemId;
            var variant = new DalVariant() { Id = Guid.NewGuid(), ItemId = originalItemId, ItemVersion = 1 };
            tests[0].Variants.Add(variant);

            _dataAccess.Save(tests[0]);

            var variantItemId2 = Guid.NewGuid();
            var variant2 = new DalVariant()
            {
                Id = Guid.NewGuid(),
                ItemId = variantItemId2,
                ItemVersion = 1,
                DalKeyConversionResults =
                    new List<DalKeyConversionResult>()
                    {
                        new DalKeyConversionResult()
                        {
                            Conversions = 1,
                            KpiId = Guid.NewGuid(),
                            Weight = .5,
                            SelectedWeight = "Medium",
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now,
                            Id = Guid.NewGuid()
                        }
                    }
            };
            tests[0].Variants.Add(variant2);

            _dataAccess.Save(tests[0]);

            Assert.Equal(originalItemId, _dataAccess.Get(tests[0].Id).OriginalItemId);
            Assert.Equal(2, _dataAccess.Get(tests[0].Id).Variants.Count);
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
                            KeyValueResults = new List<KeyValueResult>(), KeyConversionResults = new List<KeyConversionResult>() },
                        new Variant() {Id = Guid.NewGuid(), ItemVersion = 1, ItemId = itemId, Views = 5000, Conversions = 100, KeyFinancialResults = new List<KeyFinancialResult>(),
                            KeyValueResults = new List<KeyValueResult>(), KeyConversionResults = new List<KeyConversionResult>()}
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


        [Fact]
        public void TestManagerSaveAddKpiResult()
        {
            var tests = AddMultivariateTests(_dataAccess, 1);
            var originalItemId = Guid.NewGuid();
            tests[0].OriginalItemId = originalItemId;
            var variant = new DalVariant() { Id = Guid.NewGuid(), ItemId = originalItemId, ItemVersion = 1 };
            tests[0].Variants.Add(variant);

            _dataAccess.Save(tests[0]);

            var variantId = Guid.NewGuid();
            var variantItemId2 = Guid.NewGuid();
            var variant2 = new DalVariant() { Id = variantId, ItemId = variantItemId2, ItemVersion = 2 };
            tests[0].Variants.Add(variant2);

            _dataAccess.Save(tests[0]);

            Assert.Equal(originalItemId, _dataAccess.Get(tests[0].Id).OriginalItemId);
            Assert.Equal(2, _dataAccess.Get(tests[0].Id).Variants.Count);

            var result = new DalKeyFinancialResult()
            {
                Id = Guid.NewGuid(),
                Total = 12,
                KpiId = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                TotalMarketCulture = "en-US",
                ConvertedTotal = 400,
                ConvertedTotalCulture = "SEK"


            };

            var result1 = new DalKeyValueResult()
            {
                Id = Guid.NewGuid(),
                Value = 12,
                KpiId = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _dataAccess.AddKpiResultData(tests[0].Id, 1, result, 0);
            _dataAccess.AddKpiResultData(tests[0].Id, 2, result1, 1);

            Assert.Equal(1, _dataAccess.Get(tests[0].Id).Variants.First(v => v.ItemVersion == 1).DalKeyFinancialResults.Count);
            Assert.Equal(1, _dataAccess.Get(tests[0].Id).Variants.First(v => v.ItemVersion == 2).DalKeyValueResults.Count);
        }

        //[Fact]
        //public void DataAccessGetDatabaseVersion()
        //{
            
        //    _historyContext = new HistoryContext(_dbConnection, "dbo");
        //    var row = new HistoryRow();
        //    row.MigrationId = "1234_MigrationId";
        //    row.ContextKey = "ContextKey";
        //    row.ProductVersion = "1.0.0.0";
        //    row.Model = new byte[4];
        //    _historyContext.Database.Delete();
        //    _historyContext.Database.CreateIfNotExists();
        //    _historyContext.History.Add(row);
        //    _historyContext.SaveChanges();

        //    _historyDataAccess = new TestingDataAccess(new TestRepository(_historyContext));
        //    var version = _historyDataAccess.GetDatabaseVersion(_dbConnection, "dbo", "ContextKey");

        //    Assert.Equal(1234, version);
        //}
    }
}
