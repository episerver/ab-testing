using System;
using System.Runtime.Serialization;

namespace EPiServer.Marketing.KPI.Common.Attributes
{
    /// <summary>
    /// KPI Class attribute that specifies a file embedded in your assembly 
    /// to use for the configuration and readonly UI markup fragment.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UIMarkupAttribute : Attribute
    {
        /// <summary>
        /// The resource name, including namespace, for the KPI configuration markup.
        /// </summary>
        public string configmarkup { get; set; }
        /// <summary>
        /// The resource name, including namespace, for the KPI readonly markup.
        /// </summary>
        public string readonlymarkup { get; set; }
        /// <summary>
        /// Text that is the KPI object.
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// Description of the KPI object, what it does etc.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// Translation ID for text.
        /// </summary>
        public string text_id { get; set; }
       /// <summary>
       /// Translation ID for description text.
       /// </summary>
       public string description_id { get; set; }
    }
}
