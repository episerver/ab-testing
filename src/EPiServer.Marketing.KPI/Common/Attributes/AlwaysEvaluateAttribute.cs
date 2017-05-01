using System;

namespace EPiServer.Marketing.KPI.Common.Attributes
{
    /// <summary>
    /// KPI Class level attribute that specifies that KPI should always be called. If this attribute
    /// is not specified the KPI will only be called until it evaluates to true.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AlwaysEvaluateAttribute : Attribute
    {
    }
}
