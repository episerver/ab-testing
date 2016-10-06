using EPiServer.Core;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Web.Routing;

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

        public ContentComparatorKPI()
        {
        }

        public ContentComparatorKPI(Guid contentGuid)
        {
            ContentGuid = contentGuid;
        }

        public override bool Validate(Dictionary<string, string> responseData)
        {
            if (responseData["ConversionPage"] == "")
            {
                throw new KpiValidationException(ServiceLocator.Current.GetInstance<LocalizationService>().GetString("/abtesting/addtestview/error_conversionpage"));
            }

            var content = ServiceLocator.Current.GetInstance<IContentRepository>()
                    .Get<IContent>(new ContentReference(responseData["ConversionPage"]));
            if (IsContentPublished(content) && !IsCurrentContent(content))
            { ContentGuid = content.ContentGuid; }

            return true;
        }

        public override bool Evaluate(object sender, EventArgs e)
        {
            var retval = false;
            var ea = e as ContentEventArgs;
            if (ea != null)
            {
                retval = ContentGuid.Equals(ea.Content.ContentGuid);
            }
            return retval;
        }

        private bool IsContentPublished(IContent content)
        {
            IContentVersionRepository repo = ServiceLocator.Current.GetInstance<IContentVersionRepository>();
            var publishedContent = repo.LoadPublished(content.ContentLink);
            if (publishedContent == null)
            {
                throw new KpiValidationException(ServiceLocator.Current.GetInstance<LocalizationService>().GetString("/abtesting/addtestview/error_selected_notpublished"));
            }
            return true;
        }

        private bool IsCurrentContent(IContent content)
        {
            IPageRouteHelper helper = ServiceLocator.Current.GetInstance<IPageRouteHelper>();
            if (helper.PageLink.ID == content.ContentLink.ID)
            {
                throw new KpiValidationException(ServiceLocator.Current.GetInstance<LocalizationService>().GetString("/abtesting/addtestview/error_selected_samepage"));
            }
            return false;
        }
    }
}
