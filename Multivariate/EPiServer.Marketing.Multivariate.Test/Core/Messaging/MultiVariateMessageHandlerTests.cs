using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.ServiceLocation;
using Moq;
using EPiServer.Marketing.Multivariate.Messaging;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Model;
using System.Collections.Generic;

namespace EPiServer.Marketing.Multivariate.Test.Messaging
{
    [TestClass]
    public class MultiVariateMessageHandlerTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IRepository> _testrepository;
        private Mock<IMultiVariateMessageHandler> _messageHandler;

        static Guid testGuid = Guid.NewGuid();
        static Guid original = Guid.NewGuid();
        static Guid varient = Guid.NewGuid();

        MultivariateTest test = new MultivariateTest()
        {
            Id = testGuid,
            Title = "SomeTest",
            MultivariateTestResults = new List<MultivariateTestResult>() {
                new MultivariateTestResult() { ItemId = original, Conversions = 5, Views = 10 },
                new MultivariateTestResult() { ItemId = varient, Conversions = 100, Views = 200 },
                new MultivariateTestResult() { }
            }
        };

        private MultiVariateMessageHandler GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _testrepository = new Mock<IRepository>();
            _testrepository.Setup(tp => tp.GetById(It.Is<Guid>(g => g.Equals(testGuid)))).Returns(test);
            _serviceLocator.Setup(sl => sl.GetInstance<IRepository>()).Returns(_testrepository.Object);

            _messageHandler = new Mock<IMultiVariateMessageHandler>();
            _serviceLocator.Setup(sl => sl.GetInstance<IMultiVariateMessageHandler>()).Returns(_messageHandler.Object);

            return new MultiVariateMessageHandler(_serviceLocator.Object);
        }

        [TestMethod]
        public void UpdateConversionUpdatesCorrectValue()
        {
            var messageHandler = GetUnitUnderTest();

            messageHandler.Handle( new UpdateConversionsMessage() { TestId = testGuid, VariantId = varient } );

            // Verify that save is called and conversion value is correct
            _testrepository.Verify(tp => tp.Save(It.Is<IMultivariateTest>(
                aa =>  aa.MultivariateTestResults[1].Conversions == 101 )),
                Times.Once, "Repository save was not called or conversion value is not as expected");
        }

        [TestMethod]
        public void UpdateViewUpdatesCorrectValue()
        {
            var messageHandler = GetUnitUnderTest();

            messageHandler.Handle(new UpdateViewsMessage() { TestId = testGuid, VariantId = varient });

            // Verify that save is called and conversion value is correct
            _testrepository.Verify(tp => tp.Save(It.Is<IMultivariateTest>(
                aa => aa.MultivariateTestResults[1].Views == 201)),
                Times.Once, "Repository save was not called or view value is not as expected");
        }
    }
}
