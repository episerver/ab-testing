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
        private Mock<ITestDataCookieHelper> _testDataCookieHelperMock;
        private Mock<ITestManager> _testManagerMock;
        private Mock<IKpiManager> _kpiManagerMock = new Mock<IKpiManager>();

        private TestingController GetUnitUnderTest()
        {
            _mockServiceLocator = new Mock<IServiceLocator>();

            _marketingTestingRepoMock = new Mock<IMarketingTestingWebRepository>();
            _marketingTestingRepoMock.Setup(call => call.GetTestList(It.IsAny<TestCriteria>()))
                .Returns(new List<IMarketingTest>() { new ABTest() });
            _marketingTestingRepoMock.Setup(call => call.GetTestById(It.IsAny<Guid>())).Returns(new ABTest());
            _mockServiceLocator.Setup(s1 => s1.GetInstance<IMarketingTestingWebRepository>())
                .Returns(_marketingTestingRepoMock.Object);

            _messagingManagerMock = new Mock<IMessagingManager>();
            _messagingManagerMock.Setup(
                call =>
                    call.EmitKpiResultData(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<IKeyResult>(),
                        It.IsAny<KeyResultType>()));
            _mockServiceLocator.Setup(s1 => s1.GetInstance<IMessagingManager>()).Returns(_messagingManagerMock.Object);


            _testDataCookieHelperMock = new Mock<ITestDataCookieHelper>();
            _testDataCookieHelperMock.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>())).Returns(new TestDataCookie());
            _testDataCookieHelperMock.Setup(call => call.UpdateTestDataCookie(It.IsAny<TestDataCookie>()));
            _mockServiceLocator.Setup(s1 => s1.GetInstance<ITestDataCookieHelper>())
                .Returns(_testDataCookieHelperMock.Object);

            _testManagerMock = new Mock<ITestManager>();
            _testManagerMock.Setup(call => call.Get(It.IsAny<Guid>())).Returns(new ABTest());
            _mockServiceLocator.Setup(s1 => s1.GetInstance<ITestManager>()).Returns(_testManagerMock.Object);

            _mockServiceLocator.Setup(sl => sl.GetInstance<IKpiManager>()).Returns(_kpiManagerMock.Object);

            ServiceLocator.SetLocator(_mockServiceLocator.Object);

            return new TestingController()
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
            _marketingTestingRepoMock.Setup(call => call.GetTestById(It.Is<Guid>(g => g == id))).Returns(test);

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
                new KeyValuePair<string, string>("kpiId", Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>("total", "3")
            };

            _kpiManagerMock.Setup(call => call.Get(It.IsAny<Guid>())).Returns(new testFinancialKpi());

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
                new KeyValuePair<string, string>("kpiId", Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>("total", "3")
            };

            _kpiManagerMock.Setup(call => call.Get(It.IsAny<Guid>())).Returns(new Kpi());

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

            _kpiManagerMock.Setup(call => call.Get(It.IsAny<Guid>())).Returns(new Kpi());

            var data = new FormDataCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.SaveKpiResult(data);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
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

        [Fact]
        public void UpdateConversion_Returns_OK_Request()
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("testId", Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>("itemVersion", "1"),
                new KeyValuePair<string, string>("kpiId", Guid.NewGuid().ToString()),
            };

            var data = new FormDataCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.UpdateConversion(data);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void UpdateConversion_Returns_Bad_Request()
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("testId", ""),
                new KeyValuePair<string, string>("itemVersion", "1")
            };

            var data = new FormDataCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.UpdateConversion(data);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public void UpdateClientConversion_Returns_OK_Request()
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("testId", Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>("kpiId", Guid.NewGuid().ToString())
            };

            var data = new FormDataCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.UpdateClientConversion(data);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void UpdateClientConversion_Returns_Bad_Request()
        {
            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("testId", "")
            };

            var data = new FormDataCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.UpdateClientConversion(data);

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
