using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.ServiceLocation;
using Moq;
using EPiServer.Marketing.Multivariate.Web.Helpers;
using EPiServer.Core;
using System;

namespace EPiServer.Marketing.Multivariate.Test.Web
{
    [TestClass]
    public class UIHelperTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IContentRepository> _contentrepository;

        private UIHelper GetUnitUnderTest()
        {
            _contentrepository = new Mock<IContentRepository>();
            _contentrepository.Setup(cr => cr.Get<IContent>(It.IsAny<Guid>())).Throws<NotSupportedException>();
            _serviceLocator = new Mock<IServiceLocator>();
            _serviceLocator.Setup(sl => sl.GetInstance<IContentRepository>()).Returns(_contentrepository.Object);

            return new UIHelper(_serviceLocator.Object);
        }

        [TestMethod]
        public void Get_ContentCallsServiceLocator()
        {
            GetUnitUnderTest().getContent(Guid.NewGuid());
            _serviceLocator.Verify(sl => sl.GetInstance<IContentRepository>(), Times.Once, "GetInstance was never called");
        }

        [TestMethod]
        public void Get_ContentCallsContentRepository()
        {
            Guid theGuid = Guid.NewGuid();
            GetUnitUnderTest().getContent(theGuid);
            _contentrepository.Verify(cr => cr.Get<IContent>(It.Is<Guid>(arg => arg.Equals(theGuid))), Times.Once, "content repository get was never called");
        }

        [TestMethod]
        public void Get_ContentCallsContentRepositoryAndReturnsContentNotFound()
        {
            Guid theGuid = Guid.NewGuid();
            IContent content = GetUnitUnderTest().getContent(theGuid);

            _contentrepository.Verify(cr => cr.Get<IContent>(It.Is<Guid>(arg => arg.Equals(theGuid))), Times.Once, "content repository get was never called");

            // Now verify the name of the content returned (should be what the api specifies - ContentNotFound)
            Assert.AreEqual(content.Name, "ContentNotFound", false, "Name of content was unexpected");
        }
    }
}
