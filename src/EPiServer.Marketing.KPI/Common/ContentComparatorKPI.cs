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
using System.Runtime.Caching;
using EPiServer.Marketing.KPI.Common.Helpers;

namespace EPiServer.Marketing.KPI.Common
{

    /// <summary>
    /// Common KPI class that can be used to compare IContent Guid values.
    /// </summary>
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Markup.ContentComparatorConfigMarkup.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Markup.ContentComparatorReadOnlyMarkup.html",
        text_id = "/kpi/content_comparator_kpi/name",
        description_id = "/kpi/content_comparator_kpi/description")]
    public class ContentComparatorKPI : Kpi
    {
        /// <summary>
        /// ID of the content to be tested.
        /// </summary>
        [DataMember]
        public Guid ContentGuid;
        public IContent _content;
        public List<string>  _startpagepaths = new List<string>();
        private ObjectCache _cache;
        private IKpiHelper _epiHelper;

        public ContentComparatorKPI()
        {
        }

        /// <summary>
        /// ID of the content to be tested.
        /// </summary>
        /// <param name="contentGuid">ID of the content to be tested.</param>
        public ContentComparatorKPI(Guid contentGuid)
        {
            ContentGuid = contentGuid;
        }

        internal ContentComparatorKPI(IServiceLocator serviceLocator, Guid contentGuid) : this(contentGuid)
        {
            _servicelocator = serviceLocator;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        [DataMember]
        public override string UiReadOnlyMarkup
        {
            get
            {
                string markup = base.UiReadOnlyMarkup;

                if (ContentGuid != Guid.Empty)
                {
                     var conversionDescription = ServiceLocator.Current.GetInstance<LocalizationService>()
                        .GetString("/kpi/content_comparator_kpi/readonly_markup/conversion_selector_description");

                    var urlHelper = ServiceLocator.Current.GetInstance<UrlHelper>();
                    var contentRepository = ServiceLocator.Current.GetInstance<IContentRepository>();
                    var conversionContent = contentRepository.Get<IContent>(ContentGuid);
                    var conversionLink = urlHelper.ContentUrl(conversionContent.ContentLink);
                    markup = string.Format(markup, conversionDescription, conversionLink,
                        conversionContent.Name);
                }

                return markup;
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            _cache = MemoryCache.Default;
            var retval = false;

            _epiHelper = _servicelocator.GetInstance<IKpiHelper>();
            var ea = e as ContentEventArgs;
            if (ea != null)
            {
                if (_content == null)
                {
                    var contentRepo = ServiceLocator.Current.GetInstance<IContentRepository>();
                    _content = contentRepo.Get<IContent>(ContentGuid);

                    if (_cache.Contains("StartPagePaths") && _cache.Get("StartPagePaths") != null)
                    {
                        _startpagepaths = _cache.Get("StartPagePaths") as List<string>;
                        if (!_startpagepaths.Contains(_epiHelper.GetUrl(ContentReference.StartPage)))
                        {
                            _startpagepaths.Add(_epiHelper.GetUrl(ContentReference.StartPage));
                            _cache.Remove("StartPagePaths");
                            _cache.Add("SiteStart", _startpagepaths, DateTimeOffset.MaxValue);
                        }
                    }
                    else
                    {
                        _startpagepaths.Add(_epiHelper.GetUrl(ContentReference.StartPage));
                        _cache.Add("SiteStart", _startpagepaths, DateTimeOffset.MaxValue);
                    }
                }

                if ( ContentReference.StartPage.ID == _content.ContentLink.ID )
                {
                    // if the target content is the start page, we also need to check 
                    // the path to make sure its not just a request for some other static
                    // resources such as css or jscript
                    retval = (_startpagepaths.Contains(HttpContext.Current.Request.Path) 
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

        /// <inheritdoc />
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
