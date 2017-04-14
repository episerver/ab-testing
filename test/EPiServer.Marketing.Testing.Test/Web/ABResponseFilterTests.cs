using Xunit;
using EPiServer.Marketing.Testing.Web.Helpers;
using System.IO;
using System.Text;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ABResponseFilterTests
    {
        [Fact]
        public void ABResponseFilter_write_adds_to_the_buffered_html()
        {
            var aString = "testString";
            var aStringInBytes = Encoding.UTF8.GetBytes(aString);
            var aMemStream = new MemoryStream(aStringInBytes.Length + 1);
            var aFilter = new ABResponseFilter(aMemStream, aString);

            aFilter.Write(aStringInBytes, 0, 0);

            Assert.True(aFilter.bufferedHtml.Contains(aString), "string was not added to the buffered html");
        }
    }
}
