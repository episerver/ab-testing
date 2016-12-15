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
using EPiServer.Marketing.KPI.Results;
using EPiServer.Web.Routing;
using System.Web;

namespace EPiServer.Marketing.KPI.Common
{
    /// Common KPI class that can be used to compare IContent Guid values 
    /// 
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Markup.ContentComparatorConfigMarkup.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Markup.ContentComparatorReadOnlyMarkup.html",
        text_id = "/kpi/content_comparator_kpi/name", 
        description_id = "/kpi/content_comparator_kpi/description")]
    public class ContentComparatorKPI : Kpi
    {
        [DataMember]
        public Guid ContentGuid;

        public IContent _content;
        public string   _startpagepath;

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
                var conversionLabel = _servicelocator.GetInstance<LocalizationService>()
                    .GetString("/kpi/content_comparator_kpi/config_markup/conversion_label");
                return string.Format(base.UiMarkup, conversionLabel);
            }
        }

        [DataMember]
        public override string UiReadOnlyMarkup
        {
            get
            {
                string markup = base.UiReadOnlyMarkup;

                if (ContentGuid != Guid.Empty)
                {
                    var conversionHeaderText = ServiceLocator.Current.GetInstance<LocalizationService>()
                        .GetString("/kpi/content_comparator_kpi/readonly_markup/conversion_header");
                    var conversionDescription = ServiceLocator.Current.GetInstance<LocalizationService>()
                        .GetString("/kpi/content_comparator_kpi/readonly_markup/conversion_selector_description");

                    var urlHelper = ServiceLocator.Current.GetInstance<UrlHelper>();
                    var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
                    var conversionContent = contentRepository.Get<IContent>(ContentGuid);
                    var conversionLink = urlHelper.ContentUrl(conversionContent.ContentLink);
                    markup = string.Format(markup, conversionHeaderText, conversionDescription, conversionLink,
                        conversionContent.Name);
                }

                return markup;
            }
        }

        public override void Validate(Dictionary<string, string> responseData)
        {
            var contentRepo = ServiceLocator.Current.GetInstance<IContentRepository>();

            if (responseData["ConversionPage"] == "")
            {
                throw new KpiValidationException(_servicelocator.GetInstance<LocalizationService>().GetString("/kpi/content_comparator_kpi/config_markup/error_conversionpage"));
            }

            var conversionContent = contentRepo.Get<IContent>(new ContentReference(responseData["ConversionPage"]));
            var currentContent = contentRepo.Get<IContent>(new ContentReference(responseData["CurrentContent"]));
            if (IsContentPublished(conversionContent) && !IsCurrentContent(conversionContent, currentContent))
            {
                ContentGuid = conversionContent.ContentGuid;
            }
        }

        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            var retval = false;
            var ea = e as ContentEventArgs;
            if (ea != null)
            {
                if (_content == null)
                {   
                    var contentRepo = ServiceLocator.Current.GetInstance<IContentRepository>();
                    _content = contentRepo.Get<IContent>(ContentGuid);
                    _startpagepath = UrlResolver.Current.GetUrl(ContentReference.StartPage);
                }

                if ( ContentReference.StartPage.ID == _content.ContentLink.ID )
                {
                    // if the target content is the start page, we also need to check 
                    // the path to make sure its not just a request for some other static
                    // resources such as css or jscript
                    retval = (_startpagepath == HttpContext.Current.Request.Path 
                        && ContentGuid.Equals(ea.Content.ContentGuid));
                }
                else
                {
                    retval = ContentGuid.Equals(ea.Content.ContentGuid);
                }
            }

            return new KpiConversionResult() { KpiId = Id, HasConverted = retval };
        }

        private bool IsContentPublished(IContent content)
        {
            IContentVersionRepository repo = ServiceLocator.Current.GetInstance<IContentVersionRepository>();
            var publishedContent = repo.LoadPublished(content.ContentLink);
            if (publishedContent == null)
            {
                throw new KpiValidationException(_servicelocator.GetInstance<LocalizationService>().GetString("/kpi/content_comparator_kpi/config_markup/error_selected_notpublished"));
            }
            return true;
        }

        private bool IsCurrentContent(IContent conversionContent, IContent currentContent)
        {
            if (conversionContent.ContentLink.ID == currentContent.ContentLink.ID)
            {
                throw new KpiValidationException(_servicelocator.GetInstance<LocalizationService>().GetString("/kpi/content_comparator_kpi/config_markup/error_selected_samepage"));
            }
            return false;
        }

        private EventHandler<ContentEventArgs> _eh;
        public override event EventHandler EvaluateProxyEvent
        {
            add {
                _eh = new EventHandler<ContentEventArgs>(value);
                var service = _servicelocator.GetInstance<IContentEvents>();
                service.LoadedContent += _eh;
            }
            remove {
                var service = _servicelocator.GetInstance<IContentEvents>();
                service.LoadedContent -= _eh;
            }
        }
    }
}
