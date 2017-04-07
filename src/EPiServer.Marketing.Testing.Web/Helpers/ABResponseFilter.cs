using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    /// <summary>
    /// Filter to inject custom code wrappers and modifications
    /// to response stream.
    /// </summary>
    public class ABResponseFilter : Stream
    {
        private Stream stream;
        private StreamWriter streamWriter;
        internal string bufferedHtml;
        internal string _clientScript;

        public ABResponseFilter(Stream stm, string script)
        {
            stream = stm;
            _clientScript = script;
        }

        //Takes incomming response stream and injects our code
        // just before the </body> tag.
        public override void Write(byte[] buffer, int offset, int count)
        {
            //intercept the write and build the content for cases where data is chunked
            bufferedHtml += Encoding.UTF8.GetString(buffer);
        }

        //Unable to get a handle on the stream for unit testing, as it is disposed of after the streamwriter is disposed of. 
        //Can revisit to see if there is a good solution for this, excluding it from coverage for now.
        [ExcludeFromCodeCoverage]
        public override void Flush()
        {
            //transform the html and put it back into the stream
            string html = bufferedHtml;
            html = html.Replace("</body>", _clientScript + "</body>");

            using (StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8))
            {
                streamWriter.Write(html.ToCharArray(), 0, html.ToCharArray().Length);
                streamWriter.Flush();
            }
        }

        #region abstract required methods - not implemented
        [ExcludeFromCodeCoverage]
        public override bool CanRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [ExcludeFromCodeCoverage]
        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [ExcludeFromCodeCoverage]
        public override bool CanWrite
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [ExcludeFromCodeCoverage]
        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [ExcludeFromCodeCoverage]
        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        [ExcludeFromCodeCoverage]
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        [ExcludeFromCodeCoverage]
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
