using System;
using System.Collections.Generic;
using System.Web;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Linq;
using System.Runtime.Caching;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Web;

namespace EPiServer.Marketing.Testing.Web
{
    public class TestHandler
    {
        internal List<ContentReference> ProcessedContentList;
        private TestDataCookie _testData;
        private readonly MemoryCache _cachedVersion = MemoryCache.Default;
        private readonly TestManager _testManager = new TestManager();

        private static bool IsInEditOrPreivew
        {
            get
            {
                switch (EPiServer.Web.Routing.Segments.RequestSegmentContext.CurrentContextMode)
                {
                    case ContextMode.Edit:
                        return true;
                    case ContextMode.Preview:
                        return true;
                    case ContextMode.Undefined:
                        return true;
                    case ContextMode.Default:
                        return false;
                    default:
                        return false;
                }
            }
        }

        internal void Initialize()
        {
            _testData = new TestDataCookie();
            ProcessedContentList = new List<ContentReference>();

            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.LoadedContent += LoadedContent;

        }

        private void CacheVariant(Guid contentGuid)
        {
            var test = _testManager.GetTestByItemId(contentGuid).FirstOrDefault(x => x.State.Equals(TestState.Active));

            if (test != null)
            {
                var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();

                var d = contentLoader.Get<IContent>(contentGuid) as PageData;

                foreach (var variant in test.Variants)
                {
                    if (d != null)
                    {
                        var contentVersion = d.WorkPageID == 0 ? d.ContentLink.ID : d.WorkPageID;

                        if (variant.ItemVersion != contentVersion)
                        {
                            var contentToSave = d.ContentLink.CreateWritableClone();
                            contentToSave.WorkID = variant.ItemVersion;
                            var newContent = contentLoader.Get<IContent>(contentToSave) as ContentData;
                            if (newContent != null)
                            {
                                var contentToCache = (newContent.CreateWritableClone()) as PageData;
                                if (contentToCache != null)
                                {
                                    contentToCache.Status = VersionStatus.Published;
                                    contentToCache.StartPublish = DateTime.Now.AddDays(-1);
                                    contentToCache.MakeReadOnly();
                                    var cacheItemPolicy = new CacheItemPolicy
                                    {
                                        AbsoluteExpiration = DateTimeOffset.Parse(test.EndDate.ToString())
                                    };
                                    _cachedVersion.Add(contentGuid.ToString(), contentToCache, cacheItemPolicy);
                                }
                            }
                        }
                    }
                }
            }
        }



        private void LoadedContent(object sender, ContentEventArgs e)
        {
            ProcessedContentList.Add(e.ContentLink);
            PageData variant = _cachedVersion.Get(e.Content.ContentGuid.ToString()) as PageData;

            //Are we in edit mode?
            if (IsInEditOrPreivew) return;

            //is test cookie saved?

            _testData = GetTestDataFromCookie(e.Content.ContentGuid.ToString());

            if (_testData.TestContentId != Guid.Empty && _testData.TestVariantId != Guid.Empty)
            {
                //check if we get the swap
                if (_testData.ShowVariant)
                {
                    //swap it with the cached version
                    if (variant != null)
                    {
                        e.ContentLink = variant.ContentLink;
                        e.Content = variant;
                    }
                    return;
                }
            }

            if (e.Content is PageData && ProcessedContentList.Count == 1 && ContentUnderTest(e.Content.ContentGuid))
            {
                //get the cached content variant in case we need it.

                if (variant == null)
                {
                    CacheVariant(e.Content.ContentGuid);
                    variant = _cachedVersion.Get(e.Content.ContentGuid.ToString()) as PageData;
                }

                //get a new random variant. 
                Variant newVariant = GetVariant(e.Content.ContentGuid);
                _testData.TestId = GetActiveTestGuid(e.Content.ContentGuid);
                _testData.TestContentId = e.Content.ContentGuid;
                _testData.TestVariantId = newVariant.Id;


                if (newVariant.Id != Guid.Empty) //Empty = not part of the test
                {
                    var contentVersion = e.ContentLink.WorkID == 0 ? e.ContentLink.ID : e.ContentLink.WorkID;

                    if (newVariant.ItemVersion != contentVersion) //if the versions are different we are swapping
                    {
                        contentVersion = newVariant.ItemVersion;
                        //Log that we should always receive variant
                        _testData.ShowVariant = true;

                        //swap content
                        if (variant != null)
                        {
                            e.ContentLink = variant.ContentLink;
                            e.Content = variant;
                        }
                    }
                    else
                    {
                        _testData.ShowVariant = false;
                    }

                    //increment view if not already done
                    if (_testData.Viewed == false)
                    {
                        _testManager.IncrementCount(_testData.TestId, _testData.TestVariantId, contentVersion,
                            CountType.View);
                    }
                    //set viewed = true in testdata
                    _testData.Viewed = true;
                }
                else
                {
                    _testData.TestVariantId = Guid.Empty;
                }


                SaveTestDataToCookie(_testData);
            }
        }

        private void SaveTestDataToCookie(TestDataCookie testData)
        {
            HttpCookie cookieData = new HttpCookie(testData.TestContentId.ToString())
            {
                ["TestId"] = testData.TestId.ToString(),
                ["ShowVariant"] = testData.ShowVariant.ToString(),
                ["TestContentId"] = testData.TestContentId.ToString(),
                ["TestParticipant"] = testData.TestParticipant.ToString(),
                ["TestVariantId"] = testData.TestVariantId.ToString(),
                ["Viewed"] = testData.Viewed.ToString(),
                ["Converted"] = testData.Converted.ToString(),
                Expires = _testManager.Get(testData.TestId).EndDate.GetValueOrDefault()
            };
            HttpContext.Current.Response.Cookies.Add(cookieData);
        }

        private TestDataCookie GetTestDataFromCookie(string testContentId)
        {
            HttpCookie testDataCookie = HttpContext.Current.Request.Cookies.Get(testContentId);

            if (testDataCookie != null)
            {
                var cookieData = new TestDataCookie()
                {
                    TestId = Guid.Parse(testDataCookie["TestId"]),
                    ShowVariant = bool.Parse(testDataCookie["ShowVariant"]),
                    TestContentId = Guid.Parse(testDataCookie["TestContentId"]),
                    TestParticipant = bool.Parse(testDataCookie["TestParticipant"]),
                    TestVariantId = Guid.Parse(testDataCookie["TestVariantId"]),
                    Viewed = bool.Parse(testDataCookie["Viewed"]),
                    Converted = bool.Parse(testDataCookie["Converted"])

                };
                return cookieData;
            }
            return new TestDataCookie();
        }


        private Guid GetActiveTestGuid(Guid contentGuid)
        {
            Guid activeTestGuid = _testManager.GetTestByItemId(contentGuid).Where(x => x.OriginalItemId == contentGuid).Where(x => x.State == TestState.Active).Select(x => x.Id).First();
            return activeTestGuid;
        }

        private bool ContentUnderTest(Guid contentGuid)
        {
            var tests = _testManager.GetTestByItemId(contentGuid);
            if (tests.Any(x => x.State.Equals(TestState.Active)))
            {
                if (tests != null)
                {
                    if (tests.Count > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }

        public Variant GetVariant(Guid targetContentGuid)
        {
            var test = _testManager.GetTestByItemId(targetContentGuid).First(x => x.State.Equals(TestState.Active));
            return _testManager.ReturnLandingPage(test.Id);
        }

        public void Uninitialize()
        {
        }
    }



}