using System;
using System.Data.Common;
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

        //[Fact]
        //public void AddMultivariateTests()
        //{
        //    var newTests = AddMultivariateTests(_mtm, 2);
        //    _context.SaveChanges();

        //    Assert.Equal(_mtm.Get(newTests[0].Id), newTests[0]);
        //    Assert.Equal(_context.Kpis.Count(), 2);
        //}

        [Fact]
        public void TestManagerGet()
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

            _mtm.Save(kpi);

            Assert.Equal(_mtm.Get(id), kpi);
        }

        //[Fact]
        //public void TestManagerGetTestListNoFilter()
        //{
        //    var originalItemId = new Guid("818D6FDF-271A-4B8C-82FA-785780AD658B");
        //    var tests = AddMultivariateTests(_context, 2);

        //    var list = _mtm.Get(tests[0].Id);
        //    Assert.Equal(list.Id, tests[0].Id);
        //}

        //[Fact]
        //public void TestManagerSave()
        //{
        //    var tests = AddMultivariateTests(_mtm, 1);
        //    var newTitle = "newTitle";
        //    tests[0].Title = newTitle;
        //    _mtm.Save(tests[0]);

        //    Assert.Equal(_mtm.Get(tests[0].Id).Title, newTitle);
        //}

        //[Fact]
        //public void TestManagerDelete()
        //{
        //    var tests = AddMultivariateTests(_mtm, 3);

        //    _mtm.Delete(tests[0].Id);
        //    _mtm._repository.SaveChanges();

        //    Assert.Equal(_mtm._repository.GetAll().Count(), 2);
        //}

    }
}
