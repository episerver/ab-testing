using System;
using EPiServer.ServiceLocation;
using Moq;
using EPiServer.Marketing.Testing.Web;
using EPiServer.Marketing.Testing.Web.Repositories;

using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestingGadgetControllerTest
    {
        //private Mock<IServiceLocator> _serviceLocator;
        //private Mock<ITestRepository> _testRepository;

        //private TestingGadgetController GetUnitUnderTest()
        //{
        //    _serviceLocator = new Mock<IServiceLocator>();
        //    _testRepository = new Mock<ITestRepository>();
        //    _serviceLocator.Setup(sl => sl.GetInstance<ITestRepository>()).Returns(_testRepository.Object);

        //    return new TestingGadgetController(_serviceLocator.Object);
        //}


        //[Fact]
        //public void GadgetControlCallsRepositoryForIndexAction()
        //{
        //    var controller = GetUnitUnderTest();

        //    controller.Index();

        //    // Verify the testmanager was called by the repo with the proper argument
        //    _testRepository.Verify(tr => tr.GetTestList(It.IsAny<TestCriteria>()),
        //        Times.Once, "GadgetController did not call GetTestList");

        //}

        //[Fact]
        //public void GadgetControlCallsRepositoryForDetailsAction()
        //{
        //    var controller = GetUnitUnderTest();
        //    Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");

        //    controller.Details(theGuid.ToString());

        //    // Verify the testmanager was called by the repo with the proper argument
        //    _testRepository.Verify(tr => tr.GetTestById(It.Is<Guid>(guid => guid.Equals(theGuid))),
        //        Times.Once, "GadgetController did not call GetTestList");

        //}
    }
}
