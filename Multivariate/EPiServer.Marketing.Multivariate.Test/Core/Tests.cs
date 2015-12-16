using System;
using System.Data.Common;
using System.Linq;
using EPiServer.Enterprise;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestContext = EPiServer.Marketing.Multivariate.Test.Dal.TestContext;


namespace EPiServer.Marketing.Multivariate.Test.Core
{
    [TestClass]
    public class Tests : TestBase
    {
        private TestContext _context;
        private DbConnection _dbConnection;

        [SetUp]
        public void Initialization()
        {
            _dbConnection = Effort.DbConnectionFactory.CreateTransient();
            _context = new TestContext(_dbConnection);
        }

        [TestMethod]
        public void SimpleTest()
        {
            _dbConnection = Effort.DbConnectionFactory.CreateTransient();
            _context = new TestContext(_dbConnection);

            var id = new Guid();

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

            Assert.AreEqual(_context.MultivariateTests.Count(), 1);
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

    }
}
