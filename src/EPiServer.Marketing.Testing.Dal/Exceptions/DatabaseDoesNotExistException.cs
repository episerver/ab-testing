using System;

namespace EPiServer.Marketing.Testing.Dal.Exceptions
{
    public class DatabaseDoesNotExistException : Exception
    {
        public DatabaseDoesNotExistException() { }

        public DatabaseDoesNotExistException(string message) : base(message) { }

        public DatabaseDoesNotExistException(string message, Exception inner) : base(message, inner) { }
    }
}
