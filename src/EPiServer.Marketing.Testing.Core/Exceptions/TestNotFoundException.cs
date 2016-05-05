using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Core.Exceptions
{
    public class TestNotFoundException : Exception
    {
        public TestNotFoundException() { }
        public TestNotFoundException(string message) : base(message) { }
        public TestNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
