using EPiServer.Core;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web.Mvc;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Web.Mvc.Html;
using EPiServer.Web.Routing;
using EPiServer.Marketing.KPI.Results;

namespace EPiServer.Marketing.KPI.Common
{
    /// Common KPI class that can be used to compare IContent Guid values 
    /// 
    [DataContract]
    [EventSpecification(service = typeof(IContentEvents), methodname = "LoadedContent")]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Markup.ContentComparatorConfigMarkup.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Markup.ContentComparatorReadOnlyMarkup.html",
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

        [DataMember]
        public override string UiMarkup
        {
            get
            {
                string markup;

                var conversionLabel =
                    LocalizationService.Current.GetString("/kpi/content_comparator_kpi/config_markup/conversion_label");

                if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                {
                    var attr = (UIMarkupAttribute)Attribute.GetCustomAttribute(this.GetType(), typeof(UIMarkupAttribute));
                    string value;
                    if (!TryGetResourceString(attr.configmarkup, out value))
                    {
                        markup = LocalizationService.Current.GetString("/kpi/kpi_messaging/failed_to_load") + attr.readonlymarkup + ":" + value;
                    }
                    markup = string.Format(value, conversionLabel);
                }
                else
                {
                    markup = LocalizationService.Current.GetString("/kpi/kpi_messaging/UIMarkup_not_defined");
                }
                return markup;
            }
        }

        [DataMember]
        public override string UiReadOnlyMarkup
        {
            get
            {
                string markup = string.Empty;

                var conversionHeaderText = ServiceLocator.Current.GetInstance<LocalizationService>()
                   .GetString("/kpi/content_comparator_kpi/readonly_markup/conversion_header");
                var conversionDescription = ServiceLocator.Current.GetInstance<LocalizationService>()
                    .GetString("/kpi/content_comparator_kpi/readonly_markup/conversion_selector_description");
                if (ContentGuid != Guid.Empty)
                {
                    if (Attribute.IsDefined(GetType(), typeof(UIMarkupAttribute)))
                    {
                        var attr =
                            (UIMarkupAttribute)Attribute.GetCustomAttribute(GetType(), typeof(UIMarkupAttribute));
                        string value;
                        if (!TryGetResourceString(attr.readonlymarkup, out value))
                        {
                            markup = LocalizationService.Current.GetString("/kpi/kpi_messaging/failed_to_load") +
                                     attr.readonlymarkup + ":" + value;
                        }

                        var urlHelper = ServiceLocator.Current.GetInstance<UrlHelper>();
                        var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
                        var conversionContent = contentRepository.Get<IContent>(ContentGuid);
                        var conversionLink = urlHelper.ContentUrl(conversionContent.ContentLink);

                        markup = string.Format(value, conversionHeaderText, conversionDescription, conversionLink,
                            conversionContent.Name);
                    }
                    else
                    {
                        markup = LocalizationService.Current.GetString("/kpi/kpi_messaging/UIMarkup_not_defined");
                    }
                }
                return markup;
            }
        }

        public override bool Validate(Dictionary<string, string> responseData)
        {
            var contentRepo = ServiceLocator.Current.GetInstance<IContentRepository>();

            if (responseData["ConversionPage"] == "")
            {
                throw new KpiValidationException(LocalizationService.Current.GetString("/kpi/content_comparator_kpi/config_markup/error_conversionpage"));
            }

            var conversionContent = contentRepo.Get<IContent>(new ContentReference(responseData["ConversionPage"]));
            var currentContent = contentRepo.Get<IContent>(new ContentReference(responseData["CurrentContent"]));
            if (IsContentPublished(conversionContent) && !IsCurrentContent(conversionContent, currentContent))
            { ContentGuid = conversionContent.ContentGuid; }

            return true;
        }

        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            var retval = false;
            var ea = e as ContentEventArgs;
            if (ea != null)
            {
                retval = ContentGuid.Equals(ea.Content.ContentGuid);
            }

            return new KpiConversionResult() { KpiId = Id, HasConverted = retval };
        }

        private bool IsContentPublished(IContent content)
        {
            IContentVersionRepository repo = ServiceLocator.Current.GetInstance<IContentVersionRepository>();
            var publishedContent = repo.LoadPublished(content.ContentLink);
            if (publishedContent == null)
            {
                throw new KpiValidationException(ServiceLocator.Current.GetInstance<LocalizationService>().GetString("/kpi/content_comparator_kpi/config_markup/error_selected_notpublished"));
            }
            return true;
        }

        private bool IsCurrentContent(IContent conversionContent, IContent currentContent)
        {
            if (conversionContent.ContentLink.ID == currentContent.ContentLink.ID)
            {
                throw new KpiValidationException(LocalizationService.Current.GetString("/kpi/content_comparator_kpi/config_markup/error_selected_samepage"));
            }
            return false;
        }
    }
}
