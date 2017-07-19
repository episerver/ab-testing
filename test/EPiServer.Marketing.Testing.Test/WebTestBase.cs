using EPiServer.Logging;
using EPiServer.Marketing.KPI.Common.Helpers;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class WebTestBase
    {
        internal Mock<ITestManager> _mockTestManager;
        internal Mock<ILogger> _mockLogger;
        internal Mock<IServiceLocator> _mockServiceLocator;
        internal Mock<ITestResultHelper> _mockTestResultHelper;
        internal Mock<IMarketingTestingWebRepository> _mockMarketingTestingWebRepository;
        internal Mock<IKpiManager> _mockKpiManager;
        internal Mock<IHttpContextHelper> _mockHttpHelper;
        internal Mock<IEpiserverHelper> _mockEpiserverHelper;
        internal Mock<IKpiHelper> _mockKpiHelper = new Mock<IKpiHelper>();

        internal Mock<IServiceLocator> InitializeMocks()
        {
            _mockServiceLocator = new Mock<IServiceLocator>();
            _mockTestResultHelper = new Mock<ITestResultHelper>();
            _mockHttpHelper = new Mock<IHttpContextHelper>();
            _mockKpiManager = new Mock<IKpiManager>();
            _mockMarketingTestingWebRepository = new Mock<IMarketingTestingWebRepository>();
            _mockEpiserverHelper = new Mock<IEpiserverHelper>();
            _mockTestManager = new Mock<ITestManager>();

            _mockServiceLocator.Setup(sl => sl.GetInstance<ITestManager>()).Returns(_mockTestManager.Object);
            _mockServiceLocator.Setup(sl => sl.GetInstance<IKpiHelper>()).Returns(_mockKpiHelper.Object);
            _mockServiceLocator.Setup(sl => sl.GetInstance<IKpiManager>()).Returns(_mockKpiManager.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<ITestResultHelper>()).Returns(_mockTestResultHelper.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<IMarketingTestingWebRepository>()).Returns(_mockMarketingTestingWebRepository.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<IHttpContextHelper>()).Returns(_mockHttpHelper.Object);
            _mockServiceLocator.Setup(call => call.GetInstance<IEpiserverHelper>()).Returns(_mockEpiserverHelper.Object);

            return _mockServiceLocator;
        }
    }
}