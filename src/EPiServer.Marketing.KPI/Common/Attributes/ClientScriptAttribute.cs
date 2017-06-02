using System;

namespace EPiServer.Marketing.KPI.Common.Attributes
{
    /// <summary>
    /// KPI Class attribute that specifies the script to run to determine conversions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ClientScriptAttribute : Attribute
    {
        /// <summary>
        ///  The resource name, including namespace, containing the javascript necessary to process and evaluate the client side KPI conversion conditions.
        /// </summary>
        public string ClientSideEvaluationScript { get; set; }
    }
}
