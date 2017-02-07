using EPiServer.Core;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web;
using System.Runtime.Caching;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Editor;

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
                        string originalPath = (string)_sessionCache.Get(sessionid);
                        if ( requestedPage != null &&                           // we are requesting a page (not something else) And
                            httpContext.Request.Path != originalPath &&         // the page path is not the same as the page path that added the session And
                            requestedPage.ContentGuid != TestContentGuid  &&    // we are not requesting the content under test And
                            !IsSupportingContent() )                            // request is not for supporting content like css, jpeg, global or site assets
                        {
                            _sessionCache.Remove(sessionid);
                            retval = true;
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
                service.LoadedContent += AddSessionOnLoadedContent;
            }
            remove
            {
                var service = _servicelocator.GetInstance<IContentEvents>();
                service.LoadedContent -= _eh;
                service.LoadedContent -= AddSessionOnLoadedContent;
            }
        }

        public void AddSessionOnLoadedContent(object sender, ContentEventArgs e)
        {
            // we only want to evaluate once per request and add the user browser sesssion of the 
            // request to the cache, if its not there. Related to MAR-782 Kpi does not work with 
            // block testing
            var httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                var sessionid = httpContext.Request.Params["ASP.NET_SessionId"];
                if (sessionid != null && !_sessionCache.Contains(sessionid) && // we have a session and its not already in the cache
                    !httpContext.Items.Contains(Id.ToString()) && !PageEditing.PageIsInEditMode ) // this instance is not in the context and we are not in edit mode
                {
                    var currentGuid = e.Content.ContentGuid;
                    if (currentGuid != null && currentGuid == TestContentGuid)
                    {
                        CacheItemPolicy policy = new CacheItemPolicy();
                        policy.SlidingExpiration = new TimeSpan(0, Timeout, 0);
                        _sessionCache.Add(sessionid, httpContext.Request.Path, policy);

                        httpContext.Items[Id.ToString()] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the request is for an asset (such as image, css file)
        /// </summary>
        /// <returns></returns>
        private bool IsSupportingContent()
        {
            bool retval = false;
            
            if (HttpContext.Current.Request.CurrentExecutionFilePathExtension == ".png" ||
                HttpContext.Current.Request.CurrentExecutionFilePathExtension == ".css" ||
                HttpContext.Current.Request.Path.Contains(SystemContentRootNames.GlobalAssets.ToLower()) ||
                HttpContext.Current.Request.Path.Contains(SystemContentRootNames.ContentAssets.ToLower()) )
            {
                retval = true;
            }

            return retval;
        }

    }
}
