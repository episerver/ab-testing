using System;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.ServiceLocation;
using Moq;
using EPiServer.Marketing.Multivariate.Web;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Web.Models;

namespace EPiServer.Marketing.Multivariate.Test.Web
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MultivariateAdministrationControllerTest
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IMultivariateTestRepository> _testRepository;

       

        private MultivariateAdministrationController GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _testRepository = new Mock<IMultivariateTestRepository>();
            Mock<IContentRepository> repository = new Mock<IContentRepository>();
            _serviceLocator.Setup(sl => sl.GetInstance<IMultivariateTestRepository>()).Returns(_testRepository.Object);

            _serviceLocator.Setup(sl => sl.GetInstance<IContentRepository>()).Returns(repository.Object);
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

        [TestMethod]
        public void AdministrationController_CreateAction_ReturnsCreateViewWithId()
        {
            var controller = GetUnitUnderTest();
            var actionResult = controller.Create() as ViewResult;

            Assert.IsInstanceOfType(actionResult, typeof (ViewResult));

            ViewDataDictionary viewResult = controller.ViewData;

            Assert.IsTrue(viewResult.Keys.Contains("TestGuid"));

            Guid convertedGuid;

            Assert.IsTrue(Guid.TryParse(viewResult["TestGuid"].ToString(),out convertedGuid));

        }

        [TestMethod]
        public void AdministrationController_CreateWithInValidModel_CallsTestRepository_ReturnsCreateView()
        {
            var controller = GetUnitUnderTest();

            controller.ModelState.AddModelError("", "error");
            var actionResult = controller.Create(It.IsAny<MultivariateTestViewModel>()) as ViewResult;

            Assert.IsTrue(actionResult != null);
            Assert.AreEqual("Create", actionResult.ViewName);
        }

        //[TestMethod]
        //public void AdministrationController_CreateWithValidModel_CallsTestRepository_ReturnsIndex()
        //{
        //    var controller = GetUnitUnderTest();
        //    var actionResult = controller.Create(It.IsAny<MultivariateTestViewModel>()) as ViewResult;

        //    Assert.IsTrue(actionResult!=null);
        //    Assert.AreEqual("Index", actionResult.ViewName);
        //}

        [TestMethod]
        public void AdministrationController_DeleteWithId_CallsTestRepository_ReturnsIndex()
        {
            var controller = GetUnitUnderTest();
            string testGuid = Guid.NewGuid().ToString();
            var actionResult = controller.Delete(testGuid) as ViewResult;

            Assert.IsTrue(actionResult != null);
            Assert.AreEqual("Index",actionResult.ViewName);
            _testRepository.Verify(tr => tr.GetTestList(It.IsAny<MultivariateTestCriteria>()),
                Times.Once, "Controller did not call repository to populate list after Delete");

        }

        [TestMethod]
        public void AdministrationController_StopWithId_CallsTestRepository_ReturnsIndex()
        {
            var controller = GetUnitUnderTest();
            string testGuid = Guid.NewGuid().ToString();
            var actionResult = controller.Stop(testGuid) as ViewResult;

            Assert.IsTrue(actionResult!=null);
            Assert.AreEqual("Index", actionResult.ViewName);
            _testRepository.Verify(tr => tr.GetTestList(It.IsAny<MultivariateTestCriteria>()),
                Times.Once, "Controller did not call repository to populate list after Delete");

        }

    }

    
}
