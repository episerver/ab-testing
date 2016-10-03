using EPiServer.Core;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EPiServer.Marketing.KPI.Common
{
    /// Common KPI class that can be used to compare IContent Guid values 
    /// 
    [DataContract]
    [EventSpecification(service = typeof(IContentEvents), methodname = "LoadedContent")]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Markup.ContentComparatorConfigMarkup.html", 
        readonlymarkup = "EPiServer.Marketing.KPI.Markup.ContentComparatorReadOnlyMarkup.txt",
        text = "Landing Page", description = "Choose a page for conversion.")]
    public class ContentComparatorKPI : Kpi
    {
        [DataMember]
        public Guid ContentGuid;
         
        public ContentComparatorKPI() { }

        public ContentComparatorKPI(Guid contentGuid)
        {
            ContentGuid = contentGuid;
        }

        public override KpiValidationResult Validate(Dictionary<string,string> responseData)
        {
            try
            {
                var content = ServiceLocator.Current.GetInstance<IContentRepository>()
                   .Get<IContent>(new ContentReference(responseData["ConversionPage"]));
                ContentGuid = content.ContentGuid;
            }
            catch(Exception ex)
            {

            }


            return new KpiValidationResult { IsValid = true, Message = string.Empty };



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
