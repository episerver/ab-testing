using System;
using System.Runtime.Serialization;

namespace EPiServer.Marketing.KPI.Common.Attributes
{
    /// <summary>
    /// KPI Class attribute that specifies which interface to retrieve from the the service locator,
    /// and which event method to evaluate on.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EventSpecificationAttribute : Attribute
    {
        [DataMember]
        public Type service;
        [DataMember]
        public string methodname { get; set; }

        /// <summary>
        /// returns a unique identifier that represents this service/methodnmame combo
        /// </summary>
        public string key
        {
            get { return service.FullName + methodname; }
        }
    }
}
