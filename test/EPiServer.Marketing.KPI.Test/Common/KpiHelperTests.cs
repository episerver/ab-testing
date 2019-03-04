using EPiServer.Marketing.KPI.Common.Helpers;
using EPiServer.Marketing.KPI.Test.Fakes;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace EPiServer.Marketing.KPI.Test.Common
{
    public class KpiHelperTests
    {
        private IKpiHelper GetUnitUnderTest()
        {            
            return new KpiHelper();
        }
        

        [Fact]
        public void GetRequestPath_ReturnsCurrentRequestedUrlPath()
        {
            HttpContext.Current = FakeHttpContext.FakeContext("http://localhost:48594/alloy-plan/");
            var kpiHelper = GetUnitUnderTest();
            Assert.True(kpiHelper.GetRequestPath() == "/alloy-plan/");
        }

    }
}
