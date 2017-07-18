using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.Marketing.Testing.Web.Controllers;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Manager.DataClass.Enums;
using EPiServer.Marketing.KPI.Results;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestingControllerTests
    {
        private Mock<IServiceLocator> _mockServiceLocator;
        private Mock<IMarketingTestingWebRepository> _marketingTestingRepoMock;
        private Mock<IMessagingManager> _messagingManagerMock;
        private Mock<ITestDataCookieHelper> _testDataCookieHelperMock = new Mock<ITestDataCookieHelper>();
        private Mock<ITestManager> _testManagerMock;
        private Mock<IKpiWebRepository> _kpiWebRepoMock = new Mock<IKpiWebRepository>();

        private TestingController GetUnitUnderTest()
        {
            _mockServiceLocator = new Mock<IServiceLocator>();

            _marketingTestingRepoMock = new Mock<IMarketingTestingWebRepository>();
            _marketingTestingRepoMock.Setup(call => call.GetTestList(It.IsAny<TestCriteria>()))
                .Returns(new List<IMarketingTest>() { new ABTest() });
            _marketingTestingRepoMock.Setup(call => call.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ABTest());
            _mockServiceLocator.Setup(s1 => s1.GetInstance<IMarketingTestingWebRepository>())
                .Returns(_marketingTestingRepoMock.Object);

            _messagingManagerMock = new Mock<IMessagingManager>();
            _messagingManagerMock.Setup(
                call =>
                    call.EmitKpiResultData(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<IKeyResult>(),
                        It.IsAny<KeyResultType>()));
            _mockServiceLocator.Setup(s1 => s1.GetInstance<IMessagingManager>()).Returns(_messagingManagerMock.Object);

           
            _mockServiceLocator.Setup(s1 => s1.GetInstance<ITestDataCookieHelper>())
                .Returns(_testDataCookieHelperMock.Object);

            _testManagerMock = new Mock<ITestManager>();
            _testManagerMock.Setup(call => call.Get(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ABTest());
            _mockServiceLocator.Setup(s1 => s1.GetInstance<ITestManager>()).Returns(_testManagerMock.Object);

            _mockServiceLocator.Setup(sl => sl.GetInstance<IKpiWebRepository>()).Returns(_kpiWebRepoMock.Object);

            ServiceLocator.SetLocator(_mockServiceLocator.Object);

            Mock<IHttpContextHelper> contextHelper = new Mock<IHttpContextHelper>();
            return new TestingController(contextHelper.Object)
            {
                Request = new System.Net.Http.HttpRequestMessage(),
                Configuration = new HttpConfiguration()
            };
            ;
        }

        [Fact]
        public void GetAllTests_Returns_OK_Status_Result()
        {
            var controller = GetUnitUnderTest();

            var result = controller.GetAllTests();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void GetTest_Returns_Test()
        {
            var controller = GetUnitUnderTest();

            var result = controller.GetTest(Guid.NewGuid().ToString());

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void GetTest_Returns_Not_Found()
        {
            var id = Guid.NewGuid();
            ABTest test = null;

            var controller = GetUnitUnderTest();
            _marketingTestingRepoMock.Setup(call => call.GetTestById(It.Is<Guid>(g => g == id), It.IsAny<bool>())).Returns(test);

            var result = controller.GetTest(id.ToString());
            var response = result.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.Contains(id.ToString(), response.Result);
        }

        [Fact]
        public void SaveKpiResult_Financial_Returns_OK_Request()
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("testId", Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>("itemVersion", "1"),
                new KeyValuePair<string, string>("kpiId", Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f").ToString()),
                new KeyValuePair<string, string>("total", "3")
            };

            var cookie = new TestDataCookie();
            cookie.KpiConversionDictionary.Add(Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f"), false);

            _kpiWebRepoMock.Setup(call => call.GetKpiInstance(It.IsAny<Guid>())).Returns(new testFinancialKpi());
            _testDataCookieHelperMock.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>(), It.IsAny<string>())).Returns(cookie);

            var data = new FormDataCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.SaveKpiResult(data);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void SaveKpiResult_Returns_OK_Request()
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("testId", Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>("itemVersion", "1"),
                new KeyValuePair<string, string>("kpiId", Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f").ToString()),
                new KeyValuePair<string, string>("total", "3")
            };

            var cookie = new TestDataCookie();
            cookie.KpiConversionDictionary.Add(Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f"), false);

            _kpiWebRepoMock.Setup(call => call.GetKpiInstance(It.IsAny<Guid>())).Returns(new Kpi());
            _testDataCookieHelperMock.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>(), It.IsAny<string>())).Returns(cookie);

            var data = new FormDataCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.SaveKpiResult(data);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void SaveKpiResult_Returns_Bad_Request()
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("testId", ""),
                new KeyValuePair<string, string>("itemVersion", "1"),
                new KeyValuePair<string, string>("keyResultType", "1"),
                new KeyValuePair<string, string>("kpiId", Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>("total", "3")
            };

            _kpiWebRepoMock.Setup(call => call.GetKpiInstance(It.IsAny<Guid>())).Returns(new Kpi());

            var data = new FormDataCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.SaveKpiResult(data);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public void SaveKpiResult_handles_full_range_of_itemversions()
        {
            // item versions can go up to int 32 ranges
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("testId", Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>("itemVersion", "1695874"),
                new KeyValuePair<string, string>("kpiId", Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f").ToString()),
                new KeyValuePair<string, string>("total", "3")
            };

            var cookie = new TestDataCookie();
            cookie.KpiConversionDictionary.Add(Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f"), false);

            _kpiWebRepoMock.Setup(call => call.GetKpiInstance(It.IsAny<Guid>())).Returns(new Kpi());
            _testDataCookieHelperMock.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>(), It.IsAny<string>())).Returns(cookie);

            var data = new FormDataCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.SaveKpiResult(data);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void UpdateView_Returns_OK_Request()
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("testId", Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>("itemVersion", "1")
            };

            var data = new FormDataCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.UpdateView(data);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void UpdateView_Returns_Bad_Request()
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("testId", ""),
                new KeyValuePair<string, string>("itemVersion", "1")
            };

            var data = new FormDataCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.UpdateView(data);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }
    }

    class testFinancialKpi : IKpi
    {
        public DateTime CreatedDate { get; set; }

        public string Description { get; }

        public string FriendlyName { get; set; }

        public Guid Id { get; set; }

        public virtual string KpiResultType
        {
            get
            {
                return typeof(KpiFinancialResult).Name.ToString();
            }
        }

        public DateTime ModifiedDate { get; set; }

        public ResultComparison ResultComparison { get; set; }

        public string UiMarkup { get; set; }

        public string UiReadOnlyMarkup { get; set; }

        ResultComparison IKpi.ResultComparison
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler EvaluateProxyEvent;

        public IKpiResult Evaluate(object sender, EventArgs e) { return null; }

        public void Initialize() { }

        public void Uninitialize() { }

        public void Validate(Dictionary<string, string> kpiData) { }

        IKpiResult IKpi.Evaluate(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
