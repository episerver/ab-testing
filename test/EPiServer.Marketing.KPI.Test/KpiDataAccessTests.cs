using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using EPiServer.Marketing.KPI.Dal.Model;
using EPiServer.Marketing.KPI.DataAccess;
using Xunit;

namespace EPiServer.Marketing.KPI.Test
{
    public class KpiDataAccessTests : KpiTestBase
    {
        private KpiTestContext _context;
        private DbConnection _dbConnection;
        private KpiDataAccess _mtm;
        public KpiDataAccessTests()
        {
            _dbConnection = Effort.DbConnectionFactory.CreateTransient();
            _context = new KpiTestContext(_dbConnection);
            _mtm = new KpiDataAccess(new KpiTestRepository(_context));
        }

        [Fact]
        public void Kpi_DataAccess_Get_GetKpiList()
        {
            var id = Guid.NewGuid();

            var kpi = new DalKpi()
            {
                Id = id,
                ClassName = "EPiServer.Marketing.KPI.Common.ContentComparatorKPI, EPiServer.Marketing.KPI",
                Properties = "{\"ContentGuid\":\"b4d6245f-46b9-4938-b484-54b3deb80ddf\",\"UiMarkup\":\"<script>dojo.require(\\\"epi-cms/widget/ContentSelector\\\")</script>\\r\\n<p>\\r\\n    <label>Visitor navigates to page</label>\\r\\n    <span name=\\\"ConversionPage\\\"\\r\\n          id=\\\"ConversionPageWidget\\\"\\r\\n          data-dojo-type=\\\"epi-cms/widget/ContentSelector\\\"\\r\\n          data-dojo-props=\\\"repositoryKey:\'pages\',required: true, allowedTypes: [\'episerver.core.pagedata\'], allowedDndTypes: [], value: null\\\">\\r\\n        <script type=\\\"dojo/on\\\" data-dojo-event=\\\"change\\\">\\r\\n            var dependency = require(\\\"epi/dependency\\\")\\r\\n            var contextService = dependency.resolve(\\\"epi.shell.ContextService\\\");\\r\\n            var context = contextService.currentContext;\\r\\n            var currentContentInput = dojo.byId(\\\"CurrentContent\\\");\\r\\n            currentContentInput.value = context.id;\\r\\n        </script>\\r\\n    </span>\\r\\n    <div>\\r\\n        <input id=\\\"CurrentContent\\\" type=\\\"hidden\\\" name=\\\"CurrentContent\\\" />\\r\\n    </div>\\r\\n</p>\",\"UiReadOnlyMarkup\":\"<h4>Conversion goal</h4>\\r\\n<p>\\r\\n    <label>Visitor navigates to page</label><span class=\\\"dijitInline dijitReset digitIcon epi-iconPage\\\"></span><a href=\\\"/about-us/\\\" class=\\\"epi-visibleLink\\\">About us</a>\\r\\n</p>\",\"Id\":\"ee04f7a8-073d-4261-a0d4-2b36db652918\",\"ResultComparison\":0,\"FriendlyName\":\"Landing Page\",\"KpiResultType\":\"KpiConversionResult\",\"Description\":\"The chosen page is the one that a user must click on in order to count as a conversion.  Results: Views are the number of visitors that visit the page under test.  Conversions are the number of visitors that clicked through to the landing page at any point in the future while the test was running.\",\"CreatedDate\":\"2017-02-22T20:56:00.7531527Z\",\"ModifiedDate\":\"2017-02-22T20:56:00.7531527Z\",\"PreferredCommerceFormat\":{\"Id\":null,\"CommerceCulture\":\"DEFAULT\",\"preferredFormat\":null}}",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            _mtm.Save(new List<IDalKpi>() {kpi});

            Assert.Equal(_mtm.Get(id), kpi);

            Assert.Equal(1, _mtm.GetKpiList().Count);
        }


        [Fact]
        public void Kpi_DataAccess_Delete()
        {
            var id = Guid.NewGuid();

            var kpi = new DalKpi()
            {
                Id = id,
                ClassName = "EPiServer.Marketing.KPI.Common.ContentComparatorKPI, EPiServer.Marketing.KPI",
                Properties = "{\"ContentGuid\":\"b4d6245f-46b9-4938-b484-54b3deb80ddf\",\"UiMarkup\":\"<script>dojo.require(\\\"epi-cms/widget/ContentSelector\\\")</script>\\r\\n<p>\\r\\n    <label>Visitor navigates to page</label>\\r\\n    <span name=\\\"ConversionPage\\\"\\r\\n          id=\\\"ConversionPageWidget\\\"\\r\\n          data-dojo-type=\\\"epi-cms/widget/ContentSelector\\\"\\r\\n          data-dojo-props=\\\"repositoryKey:\'pages\',required: true, allowedTypes: [\'episerver.core.pagedata\'], allowedDndTypes: [], value: null\\\">\\r\\n        <script type=\\\"dojo/on\\\" data-dojo-event=\\\"change\\\">\\r\\n            var dependency = require(\\\"epi/dependency\\\")\\r\\n            var contextService = dependency.resolve(\\\"epi.shell.ContextService\\\");\\r\\n            var context = contextService.currentContext;\\r\\n            var currentContentInput = dojo.byId(\\\"CurrentContent\\\");\\r\\n            currentContentInput.value = context.id;\\r\\n        </script>\\r\\n    </span>\\r\\n    <div>\\r\\n        <input id=\\\"CurrentContent\\\" type=\\\"hidden\\\" name=\\\"CurrentContent\\\" />\\r\\n    </div>\\r\\n</p>\",\"UiReadOnlyMarkup\":\"<h4>Conversion goal</h4>\\r\\n<p>\\r\\n    <label>Visitor navigates to page</label><span class=\\\"dijitInline dijitReset digitIcon epi-iconPage\\\"></span><a href=\\\"/about-us/\\\" class=\\\"epi-visibleLink\\\">About us</a>\\r\\n</p>\",\"Id\":\"ee04f7a8-073d-4261-a0d4-2b36db652918\",\"ResultComparison\":0,\"FriendlyName\":\"Landing Page\",\"KpiResultType\":\"KpiConversionResult\",\"Description\":\"The chosen page is the one that a user must click on in order to count as a conversion.  Results: Views are the number of visitors that visit the page under test.  Conversions are the number of visitors that clicked through to the landing page at any point in the future while the test was running.\",\"CreatedDate\":\"2017-02-22T20:56:00.7531527Z\",\"ModifiedDate\":\"2017-02-22T20:56:00.7531527Z\",\"PreferredCommerceFormat\":{\"Id\":null,\"CommerceCulture\":\"DEFAULT\",\"preferredFormat\":null}}",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };

            _mtm.Save(new List<IDalKpi>() {kpi});

            Assert.Equal(1, _mtm.GetKpiList().Count);

            _mtm.Delete(id);

            Assert.Equal(0, _mtm.GetKpiList().Count);
        }
    }
}
