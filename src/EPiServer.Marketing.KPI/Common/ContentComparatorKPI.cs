using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager.DataClass;
using System;
using System.Runtime.Serialization;

namespace EPiServer.Marketing.KPI.Common
{
    /// Common KPI class that can be used to compare IContent Guid values 
    /// 
    [DataContract]
    public class ContentComparatorKPI : Kpi
    {
        [DataMember]
        public Guid ContentGuid;

        public ContentComparatorKPI() { }

        public ContentComparatorKPI(Guid contentGuid)
        {
            ContentGuid = contentGuid;
        }

        public override bool Evaluate(IContent content)
        {
            return ContentGuid.Equals(content.ContentGuid);
        }
    }
}
