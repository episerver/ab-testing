using System;

namespace EPiServer.Marketing.KPI.Exceptions
{
    public class KpiValidationException : Exception
    {
        public KpiValidationException() { }
        public KpiValidationException(string message) : base(message) { }
        public KpiValidationException(string message, Exception inner) : base(message, inner) { }
    }
}
