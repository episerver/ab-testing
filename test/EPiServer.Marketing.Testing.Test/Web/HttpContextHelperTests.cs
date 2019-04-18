using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Marketing.Testing.Web.Helpers;
using System.Collections.Generic;
using System.Web;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class HttpContextHelperTests
    {
        private const string GoodData = "testing";

        public static IEnumerable<object[]> GetBadData()
        {
            yield return new object[] { "\rbadstuffs" };
            yield return new object[] { "%0dbadstuffs" };
            yield return new object[] { "\nbadstuffs" };
            yield return new object[] { "%0abadstuffs" };
            yield return new object[] { "\r\nbadstuffs" };
        }

        [Theory]
        [MemberData(nameof(GetBadData))]
        public void GetCookieValueSplitsOff_BadData(string badData)
        {
            HttpContext.Current = FakeHttpContext.FakeContext("http://localhost:48594/alloy-plan/");
            HttpContext.Current.Response.Cookies["key"].Value = GoodData + badData;

            var helper = new HttpContextHelper();
            var value = helper.GetCookieValue("key");

            Assert.Equal(GoodData, value);
        }

        [Fact]
        public void GetCookieValue_WithNoBadData()
        {
            HttpContext.Current = FakeHttpContext.FakeContext("http://localhost:48594/alloy-plan/");
            HttpContext.Current.Response.Cookies["key"].Value = GoodData;

            var helper = new HttpContextHelper();
            var value = helper.GetCookieValue("key");

            Assert.Equal(GoodData, value);
        }

    }
}
