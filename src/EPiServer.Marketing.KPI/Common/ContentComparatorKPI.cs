using EPiServer.Core;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Manager.DataClass;
using System;
using System.Runtime.Serialization;

namespace EPiServer.Marketing.KPI.Common
{
    /// Common KPI class that can be used to compare IContent Guid values 
    /// 
    [DataContract]
    [EventSpecification(service = typeof(IContentEvents), methodname = "LoadedContent")]
    public class ContentComparatorKPI : Kpi
    {
        [DataMember]
        public Guid ContentGuid;
         
        public ContentComparatorKPI() { }

        public ContentComparatorKPI(Guid contentGuid)
        {
            ContentGuid = contentGuid;
        }

        public override bool Evaluate(object sender, EventArgs e)
        {
            bool retval = false;
            var ea = e as ContentEventArgs;
            if( ea != null  )
            {
                retval = ContentGuid.Equals(ea.Content.ContentGuid);
            }
            return retval;
        }
    }
}
