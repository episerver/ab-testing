using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using EPiServer.Enterprise;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;
using EPiServer.Marketing.Multivariate.Test.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NMemory.Linq;
using TestContext = EPiServer.Marketing.Multivariate.Test.Dal.TestContext;


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
                TestState = (int)TestState.Active,
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

    }
}
