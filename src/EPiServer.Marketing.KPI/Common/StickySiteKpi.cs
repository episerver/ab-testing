using EPiServer.Core;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web;
using System.Runtime.Caching;
using EPiServer.Framework.Localization;
using EPiServer.Web.Routing;
using EPiServer.Marketing.KPI.Exceptions;

namespace EPiServer.Marketing.KPI.Common
{
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Markup.StickySiteConfigMarkup.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Markup.StickySiteReadOnlyMarkup.html",
        text_id = "/kpi/stickysite_kpi/name", 
        description_id = "/kpi/stickysite_kpi/description")]
    public class StickySiteKpi : Kpi
    {
        private ObjectCache _sessionCache = MemoryCache.Default;

        [DataMember]
        public Guid TestContentGuid; 
        [DataMember]
        int Timeout;
        
        public StickySiteKpi()
        {
        }
        
        private IContent GetCurrentPage()
        {
            try
            {
                var pageHelper = _servicelocator.GetInstance<EPiServer.Web.Routing.IPageRouteHelper>();
                return pageHelper.Page;
            }
            catch { } // sometimes requests dont contain epi pages.

            return null;
        }

        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            var retval = false;
            
            // we only want to evaluate once per request
            var httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                var sessionid = httpContext.Request.Params["ASP.NET_SessionId"];
                if (sessionid != null && !HttpContext.Current.Items.Contains(Id.ToString()))
                {
                    if (_sessionCache.Contains(sessionid))
                    {
                        var requestedPage = GetCurrentPage();
                        bool converted = (bool)_sessionCache.Get(sessionid);
                        if (!converted && requestedPage != null && requestedPage.ContentGuid != TestContentGuid 
                            && httpContext.Request.Path == UrlResolver.Current.GetUrl(requestedPage.ContentLink))
                        {
                            _sessionCache.Remove(sessionid);
                            retval = true;
                        }
                    }
                    else
                    {
                        var loadedContentEventArgs = e as ContentEventArgs;
                        var currentGuid = loadedContentEventArgs.Content.ContentGuid;
                        if (currentGuid != null && currentGuid == TestContentGuid)
                        {
                            CacheItemPolicy policy = new CacheItemPolicy();
                            policy.SlidingExpiration = new TimeSpan(0, Timeout, 0);
                            _sessionCache.Add(sessionid, false, policy);

                            // sometimes we get called multiple time for the same request,
                            // this prevents us from evalluating true during the same request
                            httpContext.Items[Id.ToString()] = true;
                        }
                    }
                }
            }

            return new KpiConversionResult() { KpiId = Id, HasConverted = retval };
        }

        public override void Validate(Dictionary<string, string> responseData)
        {
            if ( responseData["Timeout"] == "" || responseData["CurrentContent"] =="")
            {
                // should never happen if the markup is correct
                var errormessage = _servicelocator.GetInstance<LocalizationService>()
                    .GetString("/kpi/stickysite_kpi/config_markup/error_internal");
                throw new KpiValidationException( 
                    string.Format(errormessage, "timeout=" + responseData["Timeout"] + " currentcontent=" + responseData["CurrentContent"]) );
            }

            // save the kpi arguments
            var contentRepo = _servicelocator.GetInstance<IContentRepository>();
            var currentContent = contentRepo.Get<IContent>(new ContentReference(responseData["CurrentContent"]));
            TestContentGuid = currentContent.ContentGuid;

            bool isInt = int.TryParse(responseData["Timeout"], out Timeout);
            if( !isInt || Timeout < 1 || Timeout > 60 )
            {
                throw new KpiValidationException(
                    _servicelocator.GetInstance<LocalizationService>()
                    .GetString("/kpi/stickysite_kpi/config_markup/error_invalid_timeoutvalue"));
            }

            Timeout = int.Parse(responseData["Timeout"]);
        }

        [DataMember]
        public override string UiMarkup
        {
            get
            {
                string markup = base.UiMarkup;

                var conversionLabel = _servicelocator.GetInstance<LocalizationService>()
                    .GetString("/kpi/stickysite_kpi/config_markup/conversion_label");
                return string.Format(markup, conversionLabel);
            }
        }

        [DataMember]
        public override string UiReadOnlyMarkup
        {
            get
            {
                string markup = base.UiReadOnlyMarkup;

                var conversionHeaderText = _servicelocator.GetInstance<LocalizationService>()
                    .GetString("/kpi/stickysite_kpi/readonly_markup/conversion_header");
                var conversionDescription = _servicelocator.GetInstance<LocalizationService>()
                    .GetString("/kpi/stickysite_kpi/readonly_markup/conversion_selector_description");
                conversionDescription = string.Format(conversionDescription, Timeout);
                markup = string.Format(markup, conversionHeaderText, conversionDescription);
                return markup;
            }
        }

        private EventHandler<ContentEventArgs> _eh;
        public override event EventHandler EvaluateProxyEvent
        {
            add
            {
                _eh = new EventHandler<ContentEventArgs>(value);
                var service = _servicelocator.GetInstance<IContentEvents>();
                service.LoadedContent += _eh;
            }
            remove
            {
                var service = _servicelocator.GetInstance<IContentEvents>();
                service.LoadedContent -= _eh;
            }
        }
    }
}
