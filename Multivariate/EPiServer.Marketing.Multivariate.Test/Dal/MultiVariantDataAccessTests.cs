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
            _mtm = new MultiVariantDataAccess(new TestRepository(_context));
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
    }
}
