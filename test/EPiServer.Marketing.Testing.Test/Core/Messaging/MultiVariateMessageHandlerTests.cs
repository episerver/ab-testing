using System;
using EPiServer.ServiceLocation;
using Moq;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.Marketing.Testing.Data;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.Messaging.Messages;
using Microsoft.Dnx.Testing.Abstractions;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Core.Messaging
{
    public class MultiVariateMessageHandlerTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<ITestManager> _testManager;

        static Guid testGuid = Guid.NewGuid();
        static Guid original = Guid.NewGuid();
        static Guid varient = Guid.NewGuid();
        private static int itemVersion = 1;

        ABTest test = new ABTest()
        {
            Id = testGuid,
            Title = "SomeTest",
            Variants = new List<Variant>() {
                new Variant() { ItemId = original, Conversions = 5, Views = 10 },
                new Variant() { ItemId = varient, Conversions = 100, Views = 200 }
            }
        };

        private TestingMessageHandler GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _testManager = new Mock<ITestManager>();
            _serviceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_testManager.Object);

            return new TestingMessageHandler(_serviceLocator.Object);
        }

        [Fact]
        public void UpdateConversionUpdatesCorrectValue()
        {
            var messageHandler = GetUnitUnderTest();

            messageHandler.Handle( new UpdateConversionsMessage() { TestId = testGuid, ItemVersion = itemVersion} );

            // Verify that save is called and conversion value is correct
            _testManager.Verify(tm => tm.IncrementCount(It.Is<Guid>( gg =>  gg.Equals(testGuid)),
                It.Is<int>(gg => gg.Equals(itemVersion)), 
                It.Is<CountType>(ct => ct.Equals(CountType.Conversion)), false) ,
                Times.Once, "Repository save was not called or conversion value is not as expected") ;
        }

        [Fact]
        public void UpdateViewUpdatesCorrectValue()
        {
            var messageHandler = GetUnitUnderTest();

            messageHandler.Handle(new UpdateViewsMessage() { TestId = testGuid, ItemVersion = itemVersion});
            // Verify that save is called and conversion value is correct

            _testManager.Verify(tm => tm.IncrementCount(It.Is<Guid>(gg => gg.Equals(testGuid)),
                It.Is<int>(gg => gg.Equals(itemVersion)), 
                It.Is<CountType>(ct => ct.Equals(CountType.View)), false),
                Times.Once, "Repository save was not called or view value is not as expected");

        }

        [Fact]
        public void EmitKpiResultDataCorrectValue()
        {
            var messageHandler = GetUnitUnderTest();
            var result = new KeyFinancialResult() {Total = 22};

            messageHandler.Handle(new AddKeyResultMessage() { TestId = testGuid, ItemVersion = itemVersion, Result = result, Type = 0 });
            // Verify that save is called and conversion value is correct

            _testManager.Verify(tm => tm.EmitKpiResultData(It.Is<Guid>(gg => gg.Equals(testGuid)),
                It.Is<int>(gg => gg.Equals(itemVersion)), 
                It.Is<IKeyResult>(ct => ct.Equals(result)), 0, false),
                Times.Once, "Repository save was not called or view value is not as expected");

        }
    }
}
