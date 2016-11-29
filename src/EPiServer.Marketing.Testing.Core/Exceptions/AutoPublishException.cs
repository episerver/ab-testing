using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Core.Exceptions
{
    public class AutoPublishException : Exception
    {
        public AutoPublishException() { }

        public AutoPublishException(string message) : base(message) { }

        public AutoPublishException(string message, Exception inner) : base(message, inner) { }
    }
}
