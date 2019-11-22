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
            var expectedString = "testString";
            var expectedStringBytes = Encoding.UTF8.GetBytes(expectedString);

            var abFilter = new ABResponseFilter(new MemoryStream(), expectedString, Encoding.UTF8);
            abFilter.Write(expectedStringBytes, 0, 0);

            Assert.True(abFilter.HtmlResponseStream.Contains(expectedString), "string was not added to the buffered html");
        }

        [Fact]
        public void ABResponseFilter_UsesExpectedEncoding()
        {
            var expectedString = "testString";
            var expectedStringBytes = Encoding.Unicode.GetBytes(expectedString);

            var abFilter = new ABResponseFilter(new MemoryStream(), expectedString, Encoding.Unicode);
            abFilter.Write(expectedStringBytes, 0, 0);

            // Note: the expected string will not be there if the encoding is not the same. 
            Assert.True(abFilter.HtmlResponseStream.Contains(expectedString), "string was not added to the buffered html");
        }

        [Fact]
        public void ABResponseFilter_FlushsInjectsScript()
        {
            var expectedScript = "Some useful script";
            var someReallyGreatHtml = "<html><head>" +
                "<script src=\"//ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js\" type=\"text/javascript\"></script>" +
                "</head><body>Somebody!</body></html>";

            var htmlStream = new MemoryStream();
            var abFilter = new ABResponseFilter(htmlStream, expectedScript, Encoding.Unicode, true);
            abFilter.Write(Encoding.Unicode.GetBytes(someReallyGreatHtml), 0, 0);
            abFilter.Flush();

            htmlStream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(htmlStream);
            string text = reader.ReadToEnd();

            // Note: the expected string will not be there if the encoding is not the same. 
            Assert.True(text.Contains(expectedScript), "expectedScript not in the html stream");
        }

        [Fact]
        public void ABResponseFilter_DoesNotCrashIfHtmlEmpty()
        {
            var expectedScript = "Some useful script";
            var someReallyGreatHtml = "  ";

            var htmlStream = new MemoryStream();
            var abFilter = new ABResponseFilter(htmlStream, expectedScript, Encoding.Unicode, true);
            abFilter.Write(Encoding.Unicode.GetBytes(someReallyGreatHtml), 0, 0);
            abFilter.Flush();

            htmlStream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(htmlStream);
            string text = reader.ReadToEnd();

            // Note: the expected string will not be there if the encoding is not the same. 
            Assert.False(text.Contains(expectedScript), "expectedScript not in the html stream");
        }

        [Fact]
        public void ABResponseFilter_DoesNotAddBodyIfNoIncluded()
        {
            var expectedScript = "Some useful script";
            var someReallyGreatHtml = "<html><head>" +
                 "<script src=\"//ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js\" type=\"text/javascript\"></script>" +
                 "</head></html>";

            var htmlStream = new MemoryStream();
            var abFilter = new ABResponseFilter(htmlStream, expectedScript, Encoding.Unicode, true);
            abFilter.Write(Encoding.Unicode.GetBytes(someReallyGreatHtml), 0, 0);
            abFilter.Flush();

            htmlStream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(htmlStream);
            string text = reader.ReadToEnd();

            // Note: the expected string will not be there if the encoding is not the same. 
            Assert.False(text.Contains(expectedScript), "expectedScript not in the html stream");
            // Note: the expected string will not be there if the encoding is not the same. 
            Assert.True(text.Contains(someReallyGreatHtml), "expected to have existing text");
        }

        [Fact]
        public void ABResponseFilter_DoesNotThrowExceptionIfBaseStreamIsNull()
        {
            var expectedScript = "Some useful script";
            var someReallyGreatHtml = "<html><head>" +
                 "<script src=\"//ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js\" type=\"text/javascript\"></script>" +
                 "</head></html>";

            var htmlStream = new MemoryStream();
            var abFilter = new ABResponseFilter(null, expectedScript, Encoding.Unicode, true);
            abFilter.Write(Encoding.Unicode.GetBytes(someReallyGreatHtml), 0, 0);
            abFilter.Flush();
        }
    }
}
