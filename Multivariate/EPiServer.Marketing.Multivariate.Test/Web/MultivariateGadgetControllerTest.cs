using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.ServiceLocation;
using Moq;
using EPiServer.Marketing.Multivariate.Web;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.Marketing.Multivariate.Dal;

namespace EPiServer.Marketing.Multivariate.Test.Web
{
    [TestClass]
    public class MultivariateGadgetControllerTest
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IMultivariateTestRepository> _testRepository;

        private MultivariateGadgetController GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _testRepository = new Mock<IMultivariateTestRepository>();
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestRepository>()).Returns(_testRepository.Object);

            return new MultivariateGadgetController(_serviceLocator.Object);
        }


        [TestMethod]
        public void GadgetControlCallsRepositoryForIndexAction()
        {
            var controller = GetUnitUnderTest();

            controller.Index();

            // Verify the testmanager was called by the repo with the proper argument
            _testRepository.Verify(tr=> tr.GetTestList(It.IsAny<MultivariateTestCriteria>()),
                Times.Once, "GadgetController did not call GetTestList");

        }
    }
}
