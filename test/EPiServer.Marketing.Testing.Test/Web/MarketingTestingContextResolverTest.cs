using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Context;
using EPiServer.Shell.Rest;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class MarketingTestingContextResolverTest
    {
        private Mock<ITestManager> _mockTestManager;
        private MarketingTestingContextResolver _marketingTestingContextResolver;
        private List<IMarketingTest> _activeTestDataList;
        private IMarketingTest _test;
        private IMarketingTest _testData;
        private readonly Guid _activeTestGuid = Guid.Parse("b6fc7ed9-089b-45b0-9eb1-f6de58591d32");
        private readonly Guid _inactiveTestGuid = Guid.Parse("466ad41d-7994-428f-ba65-09296b387627");
        private string _testTitle = "Unit Test Title";

        public MarketingTestingContextResolverTest()
        {
            _mockTestManager = new Mock<ITestManager>();
            _marketingTestingContextResolver = new MarketingTestingContextResolver(_mockTestManager.Object);
            _test = new ABTest() { Title = "_returnedTestTitle", State = TestState.Done, OriginalItemId = _activeTestGuid };
            _activeTestDataList = new List<IMarketingTest>() { new ABTest() { Title = _testTitle, State = TestState.Active, OriginalItemId = _activeTestGuid } };

            _mockTestManager.Setup(call => call.GetTestByItemId(It.Is<Guid>(g => g == _activeTestGuid)))
                .Returns(_activeTestDataList);

            _mockTestManager.Setup(call => call.GetTestByItemId(It.Is<Guid>(g => g == _inactiveTestGuid)))
                .Returns(new List<IMarketingTest>());

            _mockTestManager.Setup(call => call.Get(It.Is<Guid>(g => g == _activeTestGuid))).Returns(_test);
        }

        [Fact]
        public void TryResolveUri_without_id_should_return_false()
        {
            ClientContextBase context;

            var resolveUri = _marketingTestingContextResolver.TryResolveUri(CreateUri(null,null), out context);

            Assert.False(resolveUri);
            Assert.Null(context);
        }

        [Fact]
        public void TryResolveUri_without_proper_idType_should_return_false()
        {
            ClientContextBase context;

            var resolveUri = _marketingTestingContextResolver.TryResolveUri(CreateUri("badIdType", _activeTestGuid), out context);

            Assert.False(resolveUri);
            Assert.Null(context);
        }

        [Fact]
        public void TryResolveUri_without_proper_guid_should_return_false()
        {
            ClientContextBase context;

            var resolveUri = _marketingTestingContextResolver.TryResolveUri(CreateUri("testid", null), out context);

            Assert.False(resolveUri);
            Assert.Null(context);
        }

        [Fact]
        public void TryResolveUri_with_malformatted_property_string_should_return_false()
        {
            ClientContextBase context;

            var resolveUri = _marketingTestingContextResolver.TryResolveUri(CreateUri("malformattedpropertystring", _activeTestGuid), out context);

            Assert.False(resolveUri);
            Assert.Null(context);
        }

        [Fact]
        public void TryResolveUri_With_Content_Guid_Part_Of_Inactive_Test_Should_Return_False_And_Null_Context()
        {
            ClientContextBase context;
            var resolveUri = _marketingTestingContextResolver.TryResolveUri(CreateUri("contentid",_inactiveTestGuid), out context);

            _mockTestManager.Verify(call => call.GetTestByItemId(It.Is<Guid>(g => g == _inactiveTestGuid)), Times.Once, "TryResolveUir Should have called test manager but apparently did not");
            Assert.False(resolveUri);
            Assert.Null(context);
        }

        [Fact]
        public void TryResolveUri_With_Content_Guid_Part_Of_Active_Test_Should_Return_True_And_Proper_Context()
        {
            ClientContextBase context;
            var uri = CreateUri("contentid",_activeTestGuid);
            var resolveUri = _marketingTestingContextResolver.TryResolveUri(uri, out context);

            _mockTestManager.Verify(call => call.GetTestByItemId(It.Is<Guid>(g => g == _activeTestGuid)), Times.Once, "TryResolveUir Should have called test manager but apparently did not");
            Assert.True(resolveUri);
            Assert.NotNull(context);
            Assert.Equal(context.Name, _testTitle);
            Assert.Equal(context.Uri, uri);
            IMarketingTest returnedTestData = context.Data as IMarketingTest;
            Assert.NotNull(returnedTestData);
            Assert.Equal(returnedTestData.Title,_activeTestDataList[0].Title);
            Assert.Equal(returnedTestData.State, _activeTestDataList[0].State);
            Assert.Equal(returnedTestData.OriginalItemId, _activeTestDataList[0].OriginalItemId);
        }

        [Fact]
        public void TryResolveUri_With_Test_Guid_Should_Return_True_And_Proper_Context()
        {
            ClientContextBase context;
            var uri = CreateUri("testid", _activeTestGuid);
            var resolveUri = _marketingTestingContextResolver.TryResolveUri(uri, out context);

            _mockTestManager.Verify(call => call.Get(It.Is<Guid>(g => g == _activeTestGuid)), Times.Once, "TryResolveUir Should have called test manager but apparently did not");
            Assert.True(resolveUri);
            Assert.NotNull(context);
            Assert.Equal(context.Name, _test.Title);
            Assert.Equal(context.Uri, uri);
            IMarketingTest returnedTestData = context.Data as IMarketingTest;
            Assert.NotNull(returnedTestData);
            Assert.Equal(returnedTestData.Title, _test.Title);
            Assert.Equal(returnedTestData.State, _test.State);
            Assert.Equal(returnedTestData.OriginalItemId, _test.OriginalItemId);
        }


        private Uri CreateUri(string idType, Guid? contentGuid)
        {
            var guidString = contentGuid != Guid.Empty ? contentGuid.ToString() : string.Empty;
            string uriString;
            if (idType == null)
            {
                uriString = string.Empty;
            }
            else if(idType=="malformattedpropertystring")
            {
                uriString = "testid" +  guidString;
            }
            else
            {
                uriString = idType + "=" + guidString;
            }
            return new Uri(_marketingTestingContextResolver.Name+ ":///" + uriString );
        }
    }
}
