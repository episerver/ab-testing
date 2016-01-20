using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;
using EPiServer.Marketing.Multivariate.Test.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPiServer.Marketing.Multivariate.Test.Dal
{
    [TestClass]
    public class DalTests : TestBase
    {
        private TestContext _context;
        private DbConnection _dbConnection;

        [TestInitialize]
        public void Initialization()
        {
            _dbConnection = Effort.DbConnectionFactory.CreateTransient();
            _context = new TestContext(_dbConnection);
        }

        [TestMethod]
        public void AddMultivariateTest()
        {
            var newTests = AddMultivariateTests(_context, 2);
            _context.SaveChanges();

            Assert.AreEqual(_context.MultivariateTests.Count(), 2);
        }

        [TestMethod]
        public void DeleteMultivariateTest()
        {
            var newTests = AddMultivariateTests(_context, 3);
            _context.SaveChanges();

            _context.MultivariateTests.Remove(newTests[0]);
            _context.SaveChanges();

            Assert.AreEqual(_context.MultivariateTests.Count(), 2);
        }

        [TestMethod]
        public void UpdateMultivariateTest()
        {
            var id = Guid.NewGuid();

            var test = new MultivariateTest()
            {
                Id = id,
                Title = "Test",
                Owner = "me",
                OriginalItemId = new Guid(),
                TestState = TestState.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _context.MultivariateTests.Add(test);
            _context.SaveChanges();

            var newTitle = "NewTitle";
            test.Title = newTitle;
            _context.SaveChanges();

            Assert.AreEqual(_context.MultivariateTests.Find(id).Title, newTitle);
        }

        [TestMethod]
        public void AddVariantToTest()
        {
            var id = Guid.NewGuid();

            var test = new MultivariateTest()
            {
                Id = id,
                Title = "Test",
                Owner = "me",
                OriginalItemId = new Guid(),
                TestState = TestState.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Variants = new List<Variant>()
            };

            _context.MultivariateTests.Add(test);

            var variant = new Variant()
            {
                Id = Guid.NewGuid(),
                VariantId = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow
            };

            test.Variants.Add(variant);
            _context.SaveChanges();

            Assert.AreEqual(test.Variants.Count(), 1);

            Assert.AreEqual(1, _context.Variants.Count());
        }

        [TestMethod]
        public void AddConversionToTest()
        {
            var id = Guid.NewGuid();

            var test = new MultivariateTest()
            {
                Id = id,
                Title = "Test",
                Owner = "me",
                OriginalItemId = new Guid(),
                TestState = TestState.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                Conversions = new List<Conversion>()
            };

            _context.MultivariateTests.Add(test);

            var conversion = new Conversion()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ConversionString = "testing"
            };

            test.Conversions.Add(conversion);
            _context.SaveChanges();

            Assert.AreEqual(test.Conversions.Count(), 1);

            Assert.AreEqual(1, _context.Conversions.Count());
        }

        [TestMethod]
        public void AddKeyPerformanceIndicatorToTest()
        {
            var id = Guid.NewGuid();

            var test = new MultivariateTest()
            {
                Id = id,
                Title = "Test",
                Owner = "me",
                OriginalItemId = new Guid(),
                TestState = TestState.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                KeyPerformanceIndicators = new List<KeyPerformanceIndicator>()
            };

            _context.MultivariateTests.Add(test);

            var kpi = new KeyPerformanceIndicator()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                KeyPerformanceIndicatorId = Guid.NewGuid()
            };

            test.KeyPerformanceIndicators.Add(kpi);
            _context.SaveChanges();

            Assert.AreEqual(1, test.KeyPerformanceIndicators.Count());

            Assert.AreEqual(1, _context.KeyPerformanceIndicators.Count());
        }

        [TestMethod]
        public void AddTestResultToTest()
        {
            var id = Guid.NewGuid();

            var test = new MultivariateTest()
            {
                Id = id,
                Title = "Test",
                Owner = "me",
                OriginalItemId = new Guid(),
                TestState = TestState.Active,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.Now,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow,
                MultivariateTestResults = new List<MultivariateTestResult>()
            };

            _context.MultivariateTests.Add(test);

            var tr = new MultivariateTestResult()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                ItemId = Guid.NewGuid(),
                Conversions = 1,
                Views = 1
            };

            test.MultivariateTestResults.Add(tr);
            _context.SaveChanges();

            Assert.AreEqual(1, test.MultivariateTestResults.Count());

            Assert.AreEqual(1, _context.MultivariateTestsResults.Count());
        }
    }
}
