using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    /// <summary>
    /// Minimal response filter used to inject custom code kpi client scripts to the response stream.
    /// </summary>
    public class ABResponseFilter : Stream
    {
        private Stream responseFilterStream;
        private Encoding encoding;
        private bool leaveOpen;
        internal string clientScript;

        internal string HtmlResponseStream;

        /// <summary>
        /// constructor for the response filter
        /// </summary>
        /// <param name="responseFilterStream">httpcontext response filter stream</param>
        /// <param name="clientScript">the script to inject</param>
        /// <param name="encoding">the encoding of the httpcontext response</param>
        /// <param name="leaveOpen">a flag to allow unit testing to verify that the client script has been properly injected into the response</param>
        public ABResponseFilter(Stream responseFilterStream, string clientScript, Encoding encoding, bool leaveOpen = false)
        {
            this.responseFilterStream = responseFilterStream;
            this.clientScript = clientScript;
            this.encoding = encoding;
            this.leaveOpen = leaveOpen;
        }


        /// <summary>
        /// Takes incomming bytes and stores it using the specified encoding
        /// </summary>
        /// <param name="buffer">buffer of bytes representing the response</param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            //intercept the write and build the content for cases where data is chunked
            HtmlResponseStream += encoding.GetString(buffer);
        }

        /// <summary>
        /// Injects the clientscript into the response stream at the end of the body tag
        /// </summary>
        public override void Flush()
        {
            //transform the html and put it back into the stream
            if (!string.IsNullOrWhiteSpace(HtmlResponseStream))
            {
                string html = HtmlResponseStream;
                if (html.Contains("</body>"))
                {
                    html = html.Replace("</body>", clientScript + "</body>");
                }

                if (responseFilterStream != null)
                {
                    using (StreamWriter streamWriter = new StreamWriter(responseFilterStream, encoding, HtmlResponseStream.Length, leaveOpen))
                    {
                        streamWriter.Write(html.ToCharArray(), 0, html.ToCharArray().Length);
                        streamWriter.Flush();
                    }
                }
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
