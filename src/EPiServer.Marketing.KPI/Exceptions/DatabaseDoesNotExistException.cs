using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.KPI.Exceptions
{
    public class DatabaseDoesNotExistException : Exception
    {
        public DatabaseDoesNotExistException() { }

        public DatabaseDoesNotExistException(string message) : base(message) { }

        public DatabaseDoesNotExistException(string message, Exception inner) : base(message, inner) { }
    }
}
