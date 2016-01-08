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
            var helper = GetUnitUnderTest();
            Guid theGuid = new Guid("76B3BC47-01E8-4F6C-A07D-7F85976F5BE8");
            TestContent tc = new TestContent();

            _contentrepository.Setup(cr => cr.Get<IContent>(It.Is<Guid>(guid => guid.Equals(theGuid)))).Returns(tc);
            helper.getContent(theGuid);

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

        private class TestContent : IContent
        {
            public Guid ContentGuid { get; set; }
            public ContentReference ContentLink { get; set; }
            public int ContentTypeID { get; set; }
            public bool IsDeleted { get; set; }
            public string Name { get; set; }
            public ContentReference ParentLink { get; set; }
            public PropertyDataCollection Property { get; set; }
        }
    }
}
