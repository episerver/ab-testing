using System;

namespace EPiServer.Marketing.KPI.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ClientScriptAttribute : Attribute
    {
        public string ClientSideEvaluationScript { get; set; }
    }
}
