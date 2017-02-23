using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Web.Context;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Rest;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class MarketingTestingContextResolverTest
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IMarketingTestingWebRepository> _mockTestingRespository;
        private Mock<ITestingContextHelper> _mockTestingContextHelper;
        private MarketingTestingContextResolver _marketingTestingContextResolver;
        private IMarketingTest _activeTestData;
        private IMarketingTest _test;
        private readonly Guid _activeTestGuid = Guid.Parse("b6fc7ed9-089b-45b0-9eb1-f6de58591d32");
        private readonly Guid _inactiveTestGuid = Guid.Parse("466ad41d-7994-428f-ba65-09296b387627");
        private string _testTitle = "Unit Test Title";


        private MarketingTestingContextResolver GetUnitUnderTest()
        {
            _test = new ABTest() { Title = "_returnedTestTitle", State = TestState.Done, OriginalItemId = _activeTestGuid };
            _activeTestData = new ABTest()
            { Id = _activeTestGuid,
                Title = _testTitle,
                State = TestState.Active,
                OriginalItemId = _activeTestGuid,
                StartDate = DateTime.Now.AddDays(3),
                EndDate = DateTime.Now.AddDays(15),
                Owner = "Test Owner",
                Description = "Test Description",
                Variants = new List<Variant>()
            };

            _activeTestData.Variants.Add(new Variant()
            {
                Conversions = 6,
                Views = 10,
                Id = Guid.NewGuid(),
                ItemId = Guid.NewGuid(),
                ItemVersion = 5,
                TestId = _activeTestGuid
            });
            _activeTestData.Variants.Add(new Variant()
            {
                Conversions = 4,
                Views = 12,
                Id = Guid.NewGuid(),
                ItemId = Guid.NewGuid(),
                ItemVersion = 1,
                TestId = _activeTestGuid
            });


            MarketingTestingContextModel testModel = new MarketingTestingContextModel();
            


            Mock<PageData> pageData = new Mock<PageData>();
            pageData.Setup(call => call.ContentLink).Returns(new ContentReference(1));
            
            

            _serviceLocator = new Mock<IServiceLocator>();
           
            _mockTestingRespository = new Mock<IMarketingTestingWebRepository>();
           _mockTestingRespository.Setup(call => call.GetActiveTestForContent(It.Is<Guid>(g => g == _activeTestGuid)))
    .Returns(_activeTestData);
            _mockTestingRespository.Setup(call => call.GetActiveTestForContent(It.Is<Guid>(g => g == _inactiveTestGuid)))
    .Returns((ABTest)null);
            _mockTestingRespository.Setup(call => call.GetTestById(It.Is<Guid>(g => g == _activeTestGuid))).Returns(_test);

            _serviceLocator.Setup(sl => sl.GetInstance<IMarketingTestingWebRepository>()).Returns(_mockTestingRespository.Object);

           _mockTestingContextHelper = new Mock<ITestingContextHelper>();
            _mockTestingContextHelper.Setup(call => call.GenerateContextData(It.IsAny<IMarketingTest>()))
                .Returns(testModel);
           
            return new MarketingTestingContextResolver(_serviceLocator.Object,_mockTestingContextHelper.Object);
        }

        

        [Fact]
        public void TryResolveUri_without_id_should_return_false()
        {
            _marketingTestingContextResolver = GetUnitUnderTest();
            ClientContextBase context;

            var resolveUri = _marketingTestingContextResolver.TryResolveUri(CreateUri(null, null, "MarketingTestDetailsView"), out context);

            Assert.False(resolveUri);
            Assert.Null(context);
        }

        [Fact]
        public void TryResolveUri_without_proper_idType_should_return_false()
        {
            _marketingTestingContextResolver = GetUnitUnderTest();

            ClientContextBase context;

            var resolveUri = _marketingTestingContextResolver.TryResolveUri(CreateUri("badIdType", _activeTestGuid, "MarketingTestDetailsView"), out context);

            Assert.False(resolveUri);
            Assert.Null(context);
        }

        [Fact]
        public void TryResolveUri_without_proper_guid_should_return_false()
        {
            _marketingTestingContextResolver = GetUnitUnderTest();

            ClientContextBase context;

            var resolveUri = _marketingTestingContextResolver.TryResolveUri(CreateUri("testid", null, "MarketingTestDetailsView"), out context);

            Assert.False(resolveUri);
            Assert.Null(context);
        }

        [Fact]
        public void TryResolveUri_with_malformatted_property_string_should_return_false()
        {
            ClientContextBase context;
            _marketingTestingContextResolver = GetUnitUnderTest();

            var resolveUri = _marketingTestingContextResolver.TryResolveUri(CreateUri("malformattedpropertystring", _activeTestGuid, "MarketingTestDetailsView"), out context);

            Assert.False(resolveUri);
            Assert.Null(context);
        }

        [Fact]
        public void TryResolveUri_With_Content_Guid_Part_Of_Inactive_Test_Should_Return_False_And_Null_Context()
        {
            ClientContextBase context;
            _marketingTestingContextResolver = GetUnitUnderTest();

            var resolveUri = _marketingTestingContextResolver.TryResolveUri(CreateUri("contentid", _inactiveTestGuid, "MarketingTestDetailsView"), out context);

            Assert.False(resolveUri);
            Assert.Null(context);
        }

        [Fact]
        public void TryResolveUri_With_Content_Guid_Part_Of_Active_Test_Should_Return_True_And_Proper_Context()
        {
            _marketingTestingContextResolver = GetUnitUnderTest();

            ClientContextBase context;
            var uri = CreateUri("contentid", _activeTestGuid, "MarketingTestDetailsView");
            var resolveUri = _marketingTestingContextResolver.TryResolveUri(uri, out context);

            Assert.True(resolveUri);
            Assert.NotNull(context);
            Assert.Equal(context.Name, _testTitle);
            Assert.Equal(context.Uri, uri);
            MarketingTestingContextModel returnedTestData = context.Data as MarketingTestingContextModel;
            Assert.NotNull(returnedTestData);
          
        }

        [Fact]
        public void TryResolveUri_With_Test_Guid_Should_Return_True_And_Proper_Context()
        {
            _marketingTestingContextResolver = GetUnitUnderTest();

            ClientContextBase context;
            var uri = CreateUri("testid", _activeTestGuid, "MarketingTestDetailsView");
            var resolveUri = _marketingTestingContextResolver.TryResolveUri(uri, out context);

            Assert.True(resolveUri);
            Assert.NotNull(context);
            Assert.Equal(context.Name, _test.Title);
            Assert.Equal(context.Uri, uri);
            MarketingTestingContextModel returnedTestData = context.Data as MarketingTestingContextModel;
            Assert.NotNull(returnedTestData);
           
        }


        private Uri CreateUri(string idType, Guid? contentGuid, string targetView)
        {
            _marketingTestingContextResolver = GetUnitUnderTest();

            var guidString = contentGuid != Guid.Empty ? contentGuid.ToString() : string.Empty;
            string uriString;
            if (idType == null)
            {
                uriString = string.Empty;
            }
            else if (idType == "malformattedpropertystring")
            {
                uriString = "testid" + guidString + "/" + targetView;
            }
            else
            {
                uriString = idType + "=" + guidString + "/" + targetView;
            }
            return new Uri(_marketingTestingContextResolver.Name + ":///" + uriString);
        }
    }
}
