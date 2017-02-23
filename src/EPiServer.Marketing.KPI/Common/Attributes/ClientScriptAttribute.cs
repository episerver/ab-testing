using System;

namespace EPiServer.Marketing.KPI.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ClientScriptAttribute : Attribute
    {
        /// <summary>
        /// The resource name, including namespace, containing the javascript necessary
        /// to process & evaluate the client side KPI conversion conditions.
        /// </summary>
        public string ClientSideEvaluationScript { get; set; }
    }
}
