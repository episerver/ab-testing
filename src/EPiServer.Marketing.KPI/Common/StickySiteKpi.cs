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
                {
                    var sessionid = httpContext.Request.Params["ASP.NET_SessionId"];
                    if (sessionid != null )
                    {
                        if (_sessionCache.Contains(sessionid))
                        {
                            bool converted = (bool)_sessionCache.Get(sessionid);
                            if (!converted)
                            {
                                var currentpage = GetCurrentPage();
                                if (currentpage != null)
                                {
                                    if (httpContext.Request.Path == UrlResolver.Current.GetUrl(currentpage.ContentLink))
                                    {
                                        _sessionCache.Remove(sessionid);
                                        CacheItemPolicy policy = new CacheItemPolicy();
                                        policy.SlidingExpiration = new TimeSpan(0, 10, 0);
                                        _sessionCache.Add(sessionid, true, policy);
                                        retval = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            CacheItemPolicy policy = new CacheItemPolicy();
                            policy.SlidingExpiration = new TimeSpan(0, 10, 0);
                            _sessionCache.Add(sessionid, false, policy);
                        }
                    }
                }
            }

            return new KpiConversionResult() { KpiId = Id, HasConverted = retval };
        }

        public override void Validate(Dictionary<string, string> responseData)
        {
            // do nothing, we are converting on anypage or another request.
        }

        [DataMember]
        public override string UiMarkup
        {
            get
            {
                var conversionLabel = LocalizationService.Current
                    .GetString("/kpi/stickysite_kpi/description");
                return string.Format(base.UiMarkup, conversionLabel);
            }
        }

        [DataMember]
        public override string UiReadOnlyMarkup
        {
            get
            {
                string markup = base.UiReadOnlyMarkup;

                var conversionHeaderText = ServiceLocator.Current.GetInstance<LocalizationService>()
                    .GetString("/kpi/stickysite_kpi/name");
                var conversionDescription = ServiceLocator.Current.GetInstance<LocalizationService>()
                    .GetString("/kpi/stickysite_kpi/description");

                markup = string.Format(markup, conversionHeaderText, conversionDescription, "",
                        "");
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
