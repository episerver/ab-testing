using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Core.Exceptions
{
    public class SaveTestException : Exception
    {
        public SaveTestException() { }
        public SaveTestException(string message) : base(message) { }
        public SaveTestException(string message, Exception inner) : base(message, inner) { }
    }
}
