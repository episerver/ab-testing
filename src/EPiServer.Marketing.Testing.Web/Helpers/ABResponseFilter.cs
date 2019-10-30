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
        private Stream BaseStream;
        private Encoding Encoding;
        private bool LeaveOpen;
        internal string ClientScript;

        internal string HtmlResponseStream;

        /// <summary>
        /// Response filter used to inject jscript code related to KPI's into a page. 
        /// </summary>
        /// <param name="baseStream"></param>
        /// <param name="clientScript"></param>
        /// <param name="encoding"></param>
        /// <param name="leaveOpen">True for unit testing, default false</param>
        public ABResponseFilter(Stream baseStream, string clientScript, Encoding encoding, bool leaveOpen = false)
        {
            this.BaseStream = baseStream;
            this.ClientScript = clientScript;
            this.Encoding = encoding;
            this.LeaveOpen = leaveOpen;
        }

        //Takes incomming response stream and injects our code
        // just before the </body> tag.
        public override void Write(byte[] buffer, int offset, int count)
        {
            //intercept the write and build the content for cases where data is chunked
            HtmlResponseStream += Encoding.GetString(buffer);
        }

        //Unable to get a handle on the stream for unit testing, as it is disposed of after the streamwriter is disposed of. 
        //Can revisit to see if there is a good solution for this, excluding it from coverage for now.
        [ExcludeFromCodeCoverage]
        public override void Flush()
        {
            //transform the html and put it back into the stream
            if (!string.IsNullOrWhiteSpace(HtmlResponseStream))
            {
                string html = HtmlResponseStream;
                if (html.Contains("</body>"))
                {
                    html = html.Replace("</body>", ClientScript + "</body>");
                }

                if (BaseStream != null)
                {
                    using (StreamWriter streamWriter = new StreamWriter(BaseStream, Encoding, (int)BaseStream.Length + HtmlResponseStream.Length, LeaveOpen))
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
