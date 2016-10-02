using System;
using System.Runtime.Serialization;

namespace EPiServer.Marketing.KPI.Common.Attributes
{
    /// <summary>
    /// KPI Class attribute that specifies a file embedded in your assembly 
    /// to use for the configuration and readonly ui markup fragment.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UIMarkupAttribute : Attribute
    {
        /// <summary>
        /// the resource name, including namespace, for the kpi configuration markup
        /// </summary>
        public string configmarkup { get; set; }
        /// <summary>
        /// the resource name, including namespace, for the kpi readonly markup
        /// </summary>
        public string readonlymarkup { get; set; }
        /// <summary>
        /// text that is the Kpi object
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// Description of the Kpi object, what it does etc.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Translation id for text
        /// </summary>
        public string text_id { get; set; }
       /// <summary>
       /// translation id for description text
       /// </summary>
       public string description_id { get; set; }
    }
}
