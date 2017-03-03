using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.DataAccess;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.KPI.Dal.Model;
using Xunit;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Core;
using EPiServer.Data.Dynamic;
using EPiServer.Data.Dynamic.Internal;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Marketing.KPI.Results;

namespace EPiServer.Marketing.KPI.Test
{
    public class KPIManagerTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IKpiDataAccess> _kpiDataAccess;
        LocalizationService _localizationservice = LocalizationService.Current;
        private DalKpi _testKpi = new DalKpi()
        {
            ClassName = "EPiServer.Marketing.KPI.Common.ContentComparatorKPI, EPiServer.Marketing.KPI",
                Properties = "{\"ContentGuid\":\"b4d6245f-46b9-4938-b484-54b3deb80ddf\",\"UiMarkup\":\"<script>dojo.require(\\\"epi-cms/widget/ContentSelector\\\")</script>\\r\\n<p>\\r\\n    <label>Visitor navigates to page</label>\\r\\n    <span name=\\\"ConversionPage\\\"\\r\\n          id=\\\"ConversionPageWidget\\\"\\r\\n          data-dojo-type=\\\"epi-cms/widget/ContentSelector\\\"\\r\\n          data-dojo-props=\\\"repositoryKey:\'pages\',required: true, allowedTypes: [\'episerver.core.pagedata\'], allowedDndTypes: [], value: null\\\">\\r\\n        <script type=\\\"dojo/on\\\" data-dojo-event=\\\"change\\\">\\r\\n            var dependency = require(\\\"epi/dependency\\\")\\r\\n            var contextService = dependency.resolve(\\\"epi.shell.ContextService\\\");\\r\\n            var context = contextService.currentContext;\\r\\n            var currentContentInput = dojo.byId(\\\"CurrentContent\\\");\\r\\n            currentContentInput.value = context.id;\\r\\n        </script>\\r\\n    </span>\\r\\n    <div>\\r\\n        <input id=\\\"CurrentContent\\\" type=\\\"hidden\\\" name=\\\"CurrentContent\\\" />\\r\\n    </div>\\r\\n</p>\",\"UiReadOnlyMarkup\":\"<h4>Conversion goal</h4>\\r\\n<p>\\r\\n    <label>Visitor navigates to page</label><span class=\\\"dijitInline dijitReset digitIcon epi-iconPage\\\"></span><a href=\\\"/about-us/\\\" class=\\\"epi-visibleLink\\\">About us</a>\\r\\n</p>\",\"Id\":\"ee04f7a8-073d-4261-a0d4-2b36db652918\",\"ResultComparison\":0,\"FriendlyName\":\"Landing Page\",\"KpiResultType\":\"KpiConversionResult\",\"Description\":\"The chosen page is the one that a user must click on in order to count as a conversion.  Results: Views are the number of visitors that visit the page under test.  Conversions are the number of visitors that clicked through to the landing page at any point in the future while the test was running.\",\"CreatedDate\":\"2017-02-22T20:56:00.7531527Z\",\"ModifiedDate\":\"2017-02-22T20:56:00.7531527Z\",\"PreferredCommerceFormat\":{\"Id\":null,\"CommerceCulture\":\"DEFAULT\",\"preferredFormat\":null}}"
            };

        private DalKpi GetDalKpi()
        {
            return new DalKpi()
            {
                ClassName = "EPiServer.Marketing.KPI.Common.ContentComparatorKPI, EPiServer.Marketing.KPI",
                Properties = "{\"ContentGuid\":\"b4d6245f-46b9-4938-b484-54b3deb80ddf\",\"UiMarkup\":\"<script>dojo.require(\\\"epi-cms/widget/ContentSelector\\\")</script>\\r\\n<p>\\r\\n    <label>Visitor navigates to page</label>\\r\\n    <span name=\\\"ConversionPage\\\"\\r\\n          id=\\\"ConversionPageWidget\\\"\\r\\n          data-dojo-type=\\\"epi-cms/widget/ContentSelector\\\"\\r\\n          data-dojo-props=\\\"repositoryKey:\'pages\',required: true, allowedTypes: [\'episerver.core.pagedata\'], allowedDndTypes: [], value: null\\\">\\r\\n        <script type=\\\"dojo/on\\\" data-dojo-event=\\\"change\\\">\\r\\n            var dependency = require(\\\"epi/dependency\\\")\\r\\n            var contextService = dependency.resolve(\\\"epi.shell.ContextService\\\");\\r\\n            var context = contextService.currentContext;\\r\\n            var currentContentInput = dojo.byId(\\\"CurrentContent\\\");\\r\\n            currentContentInput.value = context.id;\\r\\n        </script>\\r\\n    </span>\\r\\n    <div>\\r\\n        <input id=\\\"CurrentContent\\\" type=\\\"hidden\\\" name=\\\"CurrentContent\\\" />\\r\\n    </div>\\r\\n</p>\",\"UiReadOnlyMarkup\":\"<h4>Conversion goal</h4>\\r\\n<p>\\r\\n    <label>Visitor navigates to page</label><span class=\\\"dijitInline dijitReset digitIcon epi-iconPage\\\"></span><a href=\\\"/about-us/\\\" class=\\\"epi-visibleLink\\\">About us</a>\\r\\n</p>\",\"Id\":\"ee04f7a8-073d-4261-a0d4-2b36db652918\",\"ResultComparison\":0,\"FriendlyName\":\"Landing Page\",\"KpiResultType\":\"KpiConversionResult\",\"Description\":\"The chosen page is the one that a user must click on in order to count as a conversion.  Results: Views are the number of visitors that visit the page under test.  Conversions are the number of visitors that clicked through to the landing page at any point in the future while the test was running.\",\"CreatedDate\":\"2017-02-22T20:56:00.7531527Z\",\"ModifiedDate\":\"2017-02-22T20:56:00.7531527Z\",\"PreferredCommerceFormat\":{\"Id\":null,\"CommerceCulture\":\"DEFAULT\",\"preferredFormat\":null}}"
            };
        }

        private KpiManager GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _kpiDataAccess = new Mock<IKpiDataAccess>();
            _kpiDataAccess.Setup(call => call.GetKpiList()).Returns(new List<IDalKpi>() { _testKpi });
            _kpiDataAccess.Setup(dal => dal.GetDatabaseVersion(It.IsAny<DbConnection>(), It.IsAny<string>(), It.IsAny<string>())).Returns(1);
            _serviceLocator.Setup(sl => sl.GetInstance<IKpiDataAccess>()).Returns(_kpiDataAccess.Object);
            _serviceLocator.Setup(sl => sl.GetInstance<LocalizationService>()).Returns(_localizationservice);

            // Set our mocked service locator so calls like ServiceLocator.Current work properly. 
            ServiceLocator.SetLocator(_serviceLocator.Object);
            return new KpiManager(_serviceLocator.Object);
        }

        [Fact]
        public void Get_Calls_Into_DataAccess()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            _kpiDataAccess.Setup(dal => dal.Get(It.IsAny<Guid>())).Returns(GetDalKpi());
            tm.Get(theGuid);

            _kpiDataAccess.Verify(da => da.Get(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer get was never called or Guid did not match.");
        }

        [Fact]
        public void KpiTestManager_CallsSaveWithKpi()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            var kpi = new Kpi()
            {
                Id = theGuid
            };
            tm.Save(kpi);

            _kpiDataAccess.Verify(da => da.Save(It.Is<DalKpi>(arg => arg.Id == theGuid)),
                "DataAcessLayer Save was never called or object did not match.");
        }

        [Fact]
        public void KpiTestManager_CallsDeleteWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var manager = GetUnitUnderTest();
            manager.Delete(theGuid);

            _kpiDataAccess.Verify(da => da.Delete(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer Delete was never called or Guid did not match.");
        }

        [Fact]
        public void Kpi_Success_Throws_Exception()
        {
            var kpi = new Kpi();
            var content = new Mock<IContent>();
            ContentEventArgs arg = new ContentEventArgs(new ContentReference()) { Content = content.Object };
            Assert.Throws<NotImplementedException>(() => kpi.Evaluate(this,arg));
        }

        [Fact]
        public void GetKpiList_Test()
        {
            var manager = GetUnitUnderTest();

            Assert.NotNull(manager.GetKpiList());
        }

        [Fact]
        public void GetDatabaseVersion_Test()
        {
            var manager = GetUnitUnderTest();

            Assert.Equal(1, manager.GetDatabaseVersion(new SqlConnection(), "", ""));

            manager.DatabaseNeedsConfiguring = true;
            Assert.Equal(0, manager.GetDatabaseVersion(new SqlConnection(), "", ""));
        }


        [Fact]
        public void DatabaseDoesNotExistExceptions()
        {
            var e = new DatabaseDoesNotExistException("test", new Exception());

            Assert.IsType<DatabaseDoesNotExistException>(new DatabaseDoesNotExistException());
            Assert.Equal("test", e.Message);

            var e2 = new DatabaseDoesNotExistException("testing");
            Assert.Equal("testing", e2.Message);

        }

        [Fact]
        public void KpiValidationExceptions()
        {
            var e = new KpiValidationException("test", new Exception());

            Assert.IsType<KpiValidationException>(new KpiValidationException());
            Assert.Equal("test", e.Message);

            var e2 = new KpiValidationException("testing");
            Assert.Equal("testing", e2.Message);

        }
    }
}
