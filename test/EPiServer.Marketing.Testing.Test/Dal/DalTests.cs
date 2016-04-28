using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;
using EPiServer.Marketing.Testing.Test.Core;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Dal
{
    public class DalTests : TestBase
    {
        private TestContext _context;
        private DbConnection _dbConnection;


        public DalTests()
        {
            _dbConnection = Effort.DbConnectionFactory.CreateTransient();
            _context = new TestContext(_dbConnection);
        }

        [Fact]
        public void AddMultivariateTest()
        {
            var newTests = AddMultivariateTests(_context, 2);
            _context.SaveChanges();

            Assert.Equal(_context.ABTests.Count(), 2);
        }

        [Fact]
        public void DeleteMultivariateTest()
        {
            var newTests = AddMultivariateTests(_context, 3);
            _context.SaveChanges();

            _context.ABTests.Remove(newTests[0]);
            _context.SaveChanges();

            Assert.Equal(_context.ABTests.Count(), 2);
        }

        [Fact]
        public void UpdateMultivariateTest()
        {
            var id = Guid.NewGuid();

            var test = new DalABTest()
            {
                Id = id,
                Title = "Test",
                Description = "Description",
                Owner = "me",
                OriginalItemId = new Guid(),
                State = DalTestState.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                ParticipationPercentage = 100,
                ExpectedVisitorCount = 100,
                ActualVisitorCount = 50,
                IsSignificant = false,
                ZScore = .5,
                ConfidenceLevel = .95
            };

            _context.ABTests.Add(test);
            _context.SaveChanges();

            var newTitle = "NewTitle";
            test.Title = newTitle;
            var newDesc = "NewDescription";
            test.Description = newDesc;
            _context.SaveChanges();

            Assert.Equal(_context.ABTests.Find(id).Title, newTitle);
            Assert.Equal(_context.ABTests.Find(id).Description, newDesc);
        }

        [Fact]
        public void AddVariantToTest()
        {
            var id = Guid.NewGuid();

            var test = new DalABTest()
            {
                Id = id,
                Title = "Test",
                Description = "Description",
                Owner = "me",
                OriginalItemId = new Guid(),
                State = DalTestState.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Variants = new List<DalVariant>()
            };

            _context.ABTests.Add(test);

            var variant = new DalVariant()
            {
                Id = Guid.NewGuid(),
                ItemId = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ItemVersion = 1,
                IsWinner = false
            };

            test.Variants.Add(variant);
            _context.SaveChanges();

            Assert.Equal(test.Variants.Count(), 1);

            Assert.Equal(1, _context.Variants.Count());
        }

        [Fact]
        public void AddKeyPerformanceIndicatorToTest()
        {
            var id = Guid.NewGuid();

            var test = new DalABTest()
            {
                Id = id,
                Title = "Test",
                Description = "Description",
                Owner = "me",
                OriginalItemId = new Guid(),
                State = DalTestState.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                KeyPerformanceIndicators = new List<DalKeyPerformanceIndicator>()
            };

            _context.ABTests.Add(test);

            var kpi = new DalKeyPerformanceIndicator()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                KeyPerformanceIndicatorId = Guid.NewGuid()
            };

            test.KeyPerformanceIndicators.Add(kpi);
            _context.SaveChanges();

            Assert.Equal(1, test.KeyPerformanceIndicators.Count());

            Assert.Equal(1, _context.KeyPerformanceIndicators.Count());
        }

    }
}
