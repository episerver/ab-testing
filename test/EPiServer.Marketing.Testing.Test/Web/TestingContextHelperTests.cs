using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Marketing.Testing.Web.Helpers;
using Moq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestingContextHelperTests : IDisposable
    {
        private TestingContextHelper _testingContextHelper ;

        private TestingContextHelper GetUnitUnderTest(HttpContext context)
        {
            return new TestingContextHelper(context);
        }

        [Fact]
        public void IsInSystemFolder_returns_true_if_context_is_null()
        {
            HttpContext context = null;
            _testingContextHelper = GetUnitUnderTest(context);
            var swapDisabled = _testingContextHelper.IsInSystemFolder();

            Assert.True(swapDisabled);
        }

       
        public void Dispose()
        {
            HttpContext.Current = null;
        }
    }
}
