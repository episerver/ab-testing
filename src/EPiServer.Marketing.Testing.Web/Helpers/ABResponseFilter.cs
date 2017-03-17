using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

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
        string bufferedHtml;
        private string _clientScript;

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
            bufferedHtml += System.Text.Encoding.UTF8.GetString(buffer);
        }

        public override void Flush()
        {
            //transform the html and put it back into the stream
            string html = bufferedHtml;
            html = html.Replace("</body>", _clientScript + "</body>");

            using (StreamWriter streamWriter = new StreamWriter(stream, System.Text.Encoding.UTF8))
            {
                streamWriter.Write(html.ToCharArray(), 0, html.ToCharArray().Length);
                streamWriter.Flush();
            }
        }

        #region abstract required methods - not implemented
        public override bool CanRead
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanSeek
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanWrite
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
