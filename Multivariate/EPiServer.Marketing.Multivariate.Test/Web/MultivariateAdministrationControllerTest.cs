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
    public class MultivariateAdministrationControllerTest
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IMultivariateTestRepository> _testRepository;

        private MultivariateAdministrationController GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _testRepository = new Mock<IMultivariateTestRepository>();
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestRepository>()).Returns(_testRepository.Object);

            return new MultivariateAdministrationController(_serviceLocator.Object);
        }

        [TestMethod]
        public void AdministrationController_IndexAction_CallsTestRepositoryGetTestList()
        {
            var controller = GetUnitUnderTest();

            controller.Index();

            _testRepository.Verify(x=>x.GetTestList(It.IsAny<MultivariateTestCriteria>()),
                Times.Once,"Multivariate Administration Controller Index Did Not Properly Call Repositories GetTestList");
        }

    }

    
}
