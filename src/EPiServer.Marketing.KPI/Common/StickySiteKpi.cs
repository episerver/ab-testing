using EPiServer.Core;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Runtime.Caching;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Web.Routing;
using EPiServer.ServiceLocation;
using EPiServer.DataAbstraction;
using System.Linq;
using EPiServer.Marketing.KPI.Common.Helpers;
using System.Web;

namespace EPiServer.Marketing.KPI.Common
{
    /// <summary>
    /// Converts when a user visits the content under test and then visits any other page within the same browser session.  
    /// Results: Views are the number of visitors that visited the web page.  
    /// Conversions are the number of visitors that clicked through to any other page within the specified time.
    /// </summary>
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Markup.StickySiteConfigMarkup.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Markup.StickySiteReadOnlyMarkup.html",
        text_id = "/kpi/stickysite_kpi/name",
        description_id = "/kpi/stickysite_kpi/description")]
    public class StickySiteKpi : Kpi
    {
        private ObjectCache _sessionCache = MemoryCache.Default;
        private IKpiHelper _stickyHelper;

        [DataMember]
        public Guid TestContentGuid;

        /// <summary>
        /// Number of minutes until another page is visited.
        /// </summary>
        [DataMember]
        public int Timeout;

        [ExcludeFromCodeCoverage]
        public StickySiteKpi()
        {
            _stickyHelper = new KpiHelper();
        }

        [ExcludeFromCodeCoverage]
        internal StickySiteKpi(IServiceLocator locator, IKpiHelper helper) : base(locator)
        {
            _stickyHelper = helper;
        }

        /// <inheritdoc />
        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            var hasConverted = !_stickyHelper.IsInSystemFolder() &&
                               e is ContentEventArgs eventArgs &&
                               eventArgs.Content != null &&
                               eventArgs.Content.ContentGuid != TestContentGuid &&
                               HttpContext.Current.Request.Cookies[$"SSK_{TestContentGuid}"] is HttpCookie cookie &&
                               HttpContext.Current.Request.Path != cookie["path"] &&
                               !IsSupportingContent();

            return new KpiConversionResult() { KpiId = Id, HasConverted = hasConverted };
        }

        /// <inheritdoc />
        public override void Validate(Dictionary<string, string> responseData)
        {
            if (responseData["Timeout"] == "" || responseData["CurrentContent"] == "")
            {
                // should never happen if the markup is correct
                var errormessage = _servicelocator.GetInstance<LocalizationService>()
                    .GetString("/kpi/stickysite_kpi/config_markup/error_internal");
                throw new KpiValidationException(
                    string.Format(errormessage, "timeout=" + responseData["Timeout"] + " currentcontent=" + responseData["CurrentContent"]));
            }

            // save the kpi arguments
            var contentRepo = _servicelocator.GetInstance<IContentRepository>();
            var currentContent = contentRepo.Get<IContent>(new ContentReference(responseData["CurrentContent"]));
            TestContentGuid = currentContent.ContentGuid;

            bool isInt = int.TryParse(responseData["Timeout"], out Timeout);
            if (!isInt || Timeout < 1 || Timeout > 60)
            {
                throw new KpiValidationException(
                    _servicelocator.GetInstance<LocalizationService>()
                    .GetString("/kpi/stickysite_kpi/config_markup/error_invalid_timeoutvalue"));
            }

            Timeout = int.Parse(responseData["Timeout"]);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override string UiReadOnlyMarkup
        {
            get
            {
                string markup = base.UiReadOnlyMarkup;

                var conversionDescription = _servicelocator.GetInstance<LocalizationService>()
                    .GetString("/kpi/stickysite_kpi/readonly_markup/conversion_selector_description");
                conversionDescription = string.Format(conversionDescription, Timeout);
                markup = string.Format(markup, conversionDescription);
                return markup;
            }
        }

        private EventHandler<ContentEventArgs> _eh;

        /// <inheritdoc />
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

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override void Initialize()
        {
            var service = _servicelocator.GetInstance<IContentEvents>();
            service.LoadedContent += AddSessionOnLoadedContent;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override void Uninitialize()
        {
            var service = _servicelocator.GetInstance<IContentEvents>();
            service.LoadedContent -= AddSessionOnLoadedContent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddSessionOnLoadedContent(object sender, ContentEventArgs e)
        {
            var cookieKey = $"SSK_{TestContentGuid}";
            var httpContext = HttpContext.Current;

            if (!_stickyHelper.IsInSystemFolder() && e.Content != null && e.Content.ContentGuid == TestContentGuid)
            {
                if (!httpContext.Items.Contains(cookieKey) && httpContext.Request.Cookies[cookieKey] == null)
                {
                    var path = IsSupportingContent() ? httpContext.Request.UrlReferrer.AbsolutePath : httpContext.Request.Path;
                    var contentId = TestContentGuid.ToString();
                    var cookie = new HttpCookie(cookieKey)
                    {
                        ["path"] = path,
                        ["contentguid"] = contentId,
                        Expires = DateTime.Now.AddMinutes(Timeout),
                        HttpOnly = true
                    };

                    if (!CookieExists(path, contentId) && IsContentBeingLoaded(path))
                    {
                        httpContext.Response.Cookies.Add(cookie);
                        HttpContext.Current.Items[cookieKey] = true; // we are done for this request. 
                    }
                }
            }
        }

        /// <summary>
        /// Method to determine if the loaded content method is actually being called to load the content associated with the test
        /// or if its just some other content 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsContentBeingLoaded(string path)
        {
            var cacheKey = $"SSK_{TestContentGuid}"; // use the cookie key as the cache key too. 

            HashSet<string> testcontentPaths;

            if (_sessionCache.Contains(cacheKey))
            {
                testcontentPaths = (HashSet<string>)_sessionCache.Get(cacheKey);
            }
            else 
            {
                testcontentPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                HttpContext.Current.Items[cacheKey] = true;    // we use this flag to keep us from processing more LoadedContent calls. 

                var contentRepo = _servicelocator.GetInstance<IContentRepository>();
                var content = contentRepo.Get<IContent>(TestContentGuid);
                var contentUrl = _servicelocator.GetInstance<UrlResolver>().GetUrl(content.ContentLink);
                if (contentUrl != null)
                {
                    testcontentPaths.Add(contentUrl);
                }
                else
                {
                    var parentContent = contentRepo.Get<IContent>(content.ParentLink);

                    var linkRepository = ServiceLocator.Current.GetInstance<IContentSoftLinkRepository>();
                    var referencingContentLinks = linkRepository.Load(content.ContentLink, true)
                                                                .Where(link =>
                                                                        link.SoftLinkType == ReferenceType.PageLinkReference &&
                                                                        !ContentReference.IsNullOrEmpty(link.OwnerContentLink))
                                                                .Select(link => link.OwnerContentLink)
                                                                .ToList();
                    foreach (var x in referencingContentLinks)
                    {
                        testcontentPaths.Add(UrlResolver.Current.GetUrl(x));
                    }
                }

                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(Timeout)
                };
                _sessionCache.Add(cacheKey, testcontentPaths, policy);

                HttpContext.Current.Items.Remove(cacheKey);
            }

            return testcontentPaths.Contains(path);
        }

        private bool CookieExists(string path, string contentGuid)
        {
            foreach (HttpCookie cookie in HttpContext.Current.Response.Cookies)
            {
                if (cookie.Name.Contains("SSK_") && cookie["path"] == path && cookie["contentguid"] == contentGuid)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the request is for an asset (such as image, css file)
        /// </summary>
        /// <returns></returns>
        private bool IsSupportingContent()
        {
            var pathExtensions = HttpContext.Current.Request.CurrentExecutionFilePathExtension;

            return pathExtensions == ".png" ||
                   pathExtensions == ".css" ||
                   HttpContext.Current.Request.Path.IndexOf(SystemContentRootNames.GlobalAssets, StringComparison.OrdinalIgnoreCase) > 0 ||
                   HttpContext.Current.Request.Path.IndexOf(SystemContentRootNames.ContentAssets, StringComparison.OrdinalIgnoreCase) > 0;
        }
    }
}
