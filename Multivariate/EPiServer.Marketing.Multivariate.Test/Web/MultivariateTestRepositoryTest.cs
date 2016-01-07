using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using EPiServer.Marketing.Multivariate.Model;
using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Web.Models;

namespace EPiServer.Marketing.Multivariate.Test.Web
{
    [TestClass]
    public class MultivariateTestRepositoryTest
    {
        private Mock<IServiceLocator> _serviceLocator;

        private MultivariateTestRepository GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            return new MultivariateTestRepository(_serviceLocator.Object);
        }

        [TestMethod]
        public void GetWinningTestResult_ReturnsCorrectResult()
        {
            Mock <MultivariateTestViewModel> test = new Mock<MultivariateTestViewModel>();
            var results = new List<MultivariateTestResult>();
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            results.Add(new MultivariateTestResult() { Views = 100, Conversions = 90 });
            results.Add(new MultivariateTestResult() { Views = 200, Conversions = 100 });
            results.Add(new MultivariateTestResult() { Id = theGuid, Views = 300, Conversions = 300 });
            results.Add(new MultivariateTestResult() { Views = 400, Conversions = 100 });
            results.Add(new MultivariateTestResult() {  Views = 500, Conversions = 200 });
            test.Setup(t => t.TestResults).Returns(results);

            MultivariateTestRepository repo = GetUnitUnderTest();

            var winner = repo.GetWinningTestResult(test.Object);

            Assert.AreEqual(theGuid, winner.Id);
        }
    }
}
