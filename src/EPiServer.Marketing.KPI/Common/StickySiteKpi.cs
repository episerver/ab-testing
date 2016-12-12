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
        text = "Site Stickiness", 
        description = "Converts when a user visits the content under test and then visits any other page within the same browser session.")]
    public class StickySiteKpi : Kpi
    {
        private IServiceLocator _servicelocator;
        private ObjectCache _sessionCache = MemoryCache.Default;

        [DataMember]
        public Guid TestContentGuid; 
        [DataMember]
        int Timeout;
        
        public StickySiteKpi()
        {
            _servicelocator = ServiceLocator.Current;
        }

        public StickySiteKpi(Guid contentGuid)
        {
            _servicelocator = ServiceLocator.Current;
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
                if (sessionid != null )
                {
                    var currentpage = GetCurrentPage();
                    if (_sessionCache.Contains(sessionid))
                    {
                        bool converted = (bool)_sessionCache.Get(sessionid);
                        if (!converted)
                        {
                            if (currentpage != null)
                            {
                                if (httpContext.Request.Path == UrlResolver.Current.GetUrl(currentpage.ContentLink))
                                {
                                    _sessionCache.Remove(sessionid);
                                    retval = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (currentpage != null && currentpage.ContentGuid == TestContentGuid)
                        {
                            CacheItemPolicy policy = new CacheItemPolicy();
                            policy.SlidingExpiration = new TimeSpan(0, Timeout, 0);
                            _sessionCache.Add(sessionid, false, policy);
                        }
                    }
                }
            }

            return new KpiConversionResult() { KpiId = Id, HasConverted = retval };
        }

        public override void Validate(Dictionary<string, string> responseData)
        {
            var contentRepo = _servicelocator.GetInstance<IContentRepository>();

            if ( responseData["Timeout"] == "" )
            {
                throw new KpiValidationException(_servicelocator.GetInstance<LocalizationService>().GetString("Internal error, Param missing"));
            }

            // do nothing, we are converting on anypage or another request.
            var currentContent = contentRepo.Get<IContent>(new ContentReference(responseData["CurrentContent"]));
            TestContentGuid = currentContent.ContentGuid;

            Timeout = int.Parse(responseData["Timeout"]);
        }

        [DataMember]
        public override string UiMarkup
        {
            get
            {
                string markup = base.UiMarkup;

                var conversionLabel = _servicelocator.GetInstance<LocalizationService>()
                    .GetString("/kpi/stickysite_kpi/conversion_label");
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
                    .GetString("/kpi/stickysite_kpi/name");
                var conversionDescription = _servicelocator.GetInstance<LocalizationService>()
                    .GetString("/kpi/stickysite_kpi/description");

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
