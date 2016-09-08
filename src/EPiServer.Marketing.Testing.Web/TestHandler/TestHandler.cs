using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Logging;
using System.Web;
using EPiServer.Marketing.Testing.Core.Manager;

namespace EPiServer.Marketing.Testing.Web
{
    internal class TestHandler : ITestHandler
    {
        private readonly ITestingContextHelper _contextHelper;
        private readonly ITestDataCookieHelper _testDataCookieHelper;
        private readonly ILogger _logger;
        private readonly ITestManager _testManager;
        private readonly DefaultMarketingTestingEvents _marketingTestingEvents;

        /// <summary>
        /// HTTPContext flag used to skip AB Test Processing in LoadContent event handler.
        /// </summary>
        public const string ABTestHandlerSkipFlag = "ABTestHandlerSkipFlag";

        [ExcludeFromCodeCoverage]
        public TestHandler()
        {
            _testDataCookieHelper = new TestDataCookieHelper();
            _contextHelper = new TestingContextHelper();
            _logger = LogManager.GetLogger();
            _testManager = ServiceLocator.Current.GetInstance<ITestManager>();
            _marketingTestingEvents = ServiceLocator.Current.GetInstance<DefaultMarketingTestingEvents>();

            // Setup our content events
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.LoadedChildren += LoadedChildren;
            contentEvents.LoadedContent += LoadedContent;
            contentEvents.DeletedContent += ContentEventsOnDeletedContent;
            contentEvents.DeletingContentVersion += ContentEventsOnDeletingContentVersion;
        }
        
        //To support unit testing
        internal TestHandler(IServiceLocator serviceLocator, ITestDataCookieHelper cookieHelper, ITestingContextHelper contextHelper, ILogger logger)
        {
            _testDataCookieHelper = cookieHelper;
            _testManager = serviceLocator.GetInstance<ITestManager>();
            _contextHelper = contextHelper;
            _logger = logger;
            _marketingTestingEvents = serviceLocator.GetInstance<DefaultMarketingTestingEvents>();
        }

        /// <summary>
        /// need this for deleted drafts as they are permanently deleted and do not go to the trash
        /// the OnDeletedContentVersion event is too late to get the guid to see if it is part of a test or not.
        /// Excluding from coverage as CheckForActiveTest is tested separately and the rest of this would be mocked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="contentEventArgs"></param>
        [ExcludeFromCodeCoverage]
        internal void ContentEventsOnDeletingContentVersion(object sender, ContentEventArgs contentEventArgs)
        {
            var serviceLocator = ServiceLocator.Current;
            var repo = serviceLocator.GetInstance<IContentRepository>();

            IContent draftContent;

            // get the actual content item so we can get its Guid to check against our tests
            if (repo.TryGet(contentEventArgs.ContentLink, out draftContent))
            {
                CheckForActiveTests(draftContent.ContentGuid, contentEventArgs.ContentLink.WorkID);
            }
        }

        /// <summary>
        /// need this for deleted published pages, this is called when the trash is emptied
        /// Excluding from coverage as CheckForActiveTest is tested separately and the rest of this would be mocked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deleteContentEventArgs"></param>
        [ExcludeFromCodeCoverage]
        internal void ContentEventsOnDeletedContent(object sender, DeleteContentEventArgs deleteContentEventArgs)
        {
            // this is the list of pages that are being deleted from the trash.  All we have is the guid, at this point in time
            // the items already seem to be gone.  Luckily all we need is the guid as this only fires for published pages.
            var guids = (List<Guid>)deleteContentEventArgs.Items["DeletedItemGuids"];

            foreach (var guid in guids)
            {
                CheckForActiveTests(guid, 0);
            }
        }

        /// <summary>
        /// Check the guid passed in to see if the page/draft is part of a test.  For published pages, the version passed in will be 0, as all we need/get is the guid
        /// for drafts, we the guid and version will be passed in to compare against known variants being tested.
        /// </summary>
        /// <param name="contentGuid">Guid of item being deleted.</param>
        /// <param name="contentVersion">0 if published page, workID if draft</param>
        /// <returns>Number of active tests that were deleted from the system.</returns>
        internal int CheckForActiveTests(Guid contentGuid, int contentVersion)
        {
            var testsDeleted = 0;
            var tests = _testManager.GetActiveTestsByOriginalItemId(contentGuid);

            // no tests found for the deleted content
            if (tests.IsNullOrEmpty())
            {
                return testsDeleted;
            }

            foreach (var test in tests)
            {
                // the published page is being deleted
                if (contentVersion == 0)
                {
                    _testManager.Stop(test.Id);
                    _testManager.Delete(test.Id);
                    testsDeleted++;
                    continue;
                }

                // a draft version of a page is being deleted
                if (test.Variants.All(v => v.ItemVersion != contentVersion))
                    continue;

                _testManager.Stop(test.Id);
                _testManager.Delete(test.Id);
                testsDeleted++;
            }
            return testsDeleted;
        }

        /// <summary>
        /// Event handler to swap out content when children are loaded, however this does not
        /// cause a conversion or view, simply creates cookie if needed and swaps content
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void LoadedChildren(object sender, ChildrenEventArgs e)
        {
            if (!_contextHelper.SwapDisabled(e))
            {
                Boolean modified = false;
                IList<IContent> childList = new List<IContent>();

                // its possible that something in the children changed, so we need to replace it with a variant 
                // if its in test. This method gets called once after the main page is loaded. (i.e. this is how
                // the links at the top of alloy get created)
                foreach (var content in e.ChildrenItems)
                {
                    try
                    {
                        // get the test from the cache
                        var activeTest = _testManager.GetActiveTestsByOriginalItemId(content.ContentGuid).FirstOrDefault();
                        if (activeTest != null)
                        {
                            var testCookieData = _testDataCookieHelper.GetTestDataFromCookie(content.ContentGuid.ToString());
                            var hasData = _testDataCookieHelper.HasTestData(testCookieData);
                            var contentVersion = content.ContentLink.WorkID == 0 ? content.ContentLink.ID :
                                content.ContentLink.WorkID;

                            if (!hasData)
                            {
                                // Make sure the cookie has data in it. There are cases where you can load
                                // content directly from a url after opening a browser and if the cookie is not set
                                // the first pass through you end up seeing original content not content under test.
                                SetTestData(content, activeTest, testCookieData, contentVersion, out testCookieData, out contentVersion);
                            }

                            if (testCookieData.ShowVariant && _testDataCookieHelper.IsTestParticipant(testCookieData))
                            {
                                modified = true;
                                childList.Add(_testManager.GetVariantContent(content.ContentGuid));
                            }
                            else
                            {
                                childList.Add(content);
                            }
                        }
                        else
                        {
                            childList.Add(content);
                        }
                    }
                    catch (Exception err)
                    {
                        _logger.Error("TestHandler.LoadChildren", err);
                    }
                }

                // if we modified the data, update the children list. Note that original order
                // is important else links do not show up in same order.
                if (modified)
                {
                    e.ChildrenItems.Clear();
                    e.ChildrenItems.AddRange(childList);
                }
            }
        }

        /// Main worker method.  Processes each content which triggers a
        /// content loaded event to determine the state of a test and what content to display.
        public void LoadedContent(object sender, ContentEventArgs e)
        {
            if (!_contextHelper.SwapDisabled(e))
            {
                try
                {
                    EvaluateCookies(e);
                    EvaluateKpis(e);

                    // get the test from the cache
                    var activeTest = _testManager.GetActiveTestsByOriginalItemId(e.Content.ContentGuid).FirstOrDefault();
                    if (activeTest != null)
                    {
                        var testCookieData = _testDataCookieHelper.GetTestDataFromCookie(e.Content.ContentGuid.ToString());
                        var hasData = _testDataCookieHelper.HasTestData(testCookieData);
                        var originalContent = e.Content;
                        var contentVersion = e.ContentLink.WorkID == 0 ? e.ContentLink.ID : e.ContentLink.WorkID;

                        // Preload the cache if needed. Note that this causes an extra call to loadContent Event
                        // so set the skip flag so we dont try to process the test.
                        HttpContext.Current.Items[ABTestHandlerSkipFlag] = true;
                        _testManager.GetVariantContent(e.Content.ContentGuid);
                        HttpContext.Current.Items.Remove(ABTestHandlerSkipFlag);

                        if (!hasData)
                        {
                            // Make sure the cookie has data in it.
                            SetTestData(e.Content, activeTest, testCookieData, contentVersion, out testCookieData, out contentVersion);
                        }

                        Swap(testCookieData, activeTest, e);
                        EvaluateViews(testCookieData, contentVersion, originalContent);
                    }
                }
                catch (Exception err)
                {
                    _logger.Error("TestHandler.LoadedContent", err);
                }
            }
        }

        private void SetTestData(IContent e, IMarketingTest activeTest, TestDataCookie testCookieData, int contentVersion, out TestDataCookie retCookieData, out int retContentVersion)
        {
            var newVariant = _testManager.ReturnLandingPage(activeTest.Id);
            testCookieData.TestId = activeTest.Id;
            testCookieData.TestContentId = activeTest.OriginalItemId;
            testCookieData.TestVariantId = newVariant.Id;

            foreach (var kpi in activeTest.KpiInstances)
            {
                testCookieData.KpiConversionDictionary.Add(kpi.Id, false);
            }

            if (newVariant.Id != Guid.Empty)
            {
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.UserIncludedInTestEvent, new TestEventArgs(activeTest, e));
                if (newVariant.ItemVersion != contentVersion)
                {
                    contentVersion = newVariant.ItemVersion;
                    testCookieData.ShowVariant = true;
                }
            }
            _testDataCookieHelper.UpdateTestDataCookie(testCookieData);
            retCookieData = testCookieData;
            retContentVersion = contentVersion;
        }
        //Handles the swapping of content data
        private void Swap(TestDataCookie cookie, IMarketingTest activeTest, ContentEventArgs activeContent)
        {
            if (cookie.ShowVariant && _testDataCookieHelper.IsTestParticipant(cookie))
            {
                var variant = _testManager.GetVariantContent(activeContent.Content.ContentGuid);
                //swap it with the cached version
                if (variant != null)
                {
                    activeContent.ContentLink = variant.ContentLink;
                    activeContent.Content = variant;

                    if (!HttpContext.Current.Items.Contains(activeContent + "skipevent"))
                    {
                        _marketingTestingEvents.RaiseMarketingTestingEvent(
                            DefaultMarketingTestingEvents.ContentSwitchedEvent,
                            new TestEventArgs(activeTest, activeContent.Content));
                        HttpContext.Current.Items[activeContent + "skipevent"] = true;
                    }
                }
            }
        }

        //Handles the incrementing of view counts on a version
        private void EvaluateViews(TestDataCookie cookie, int contentVersion, IContent originalContent)
        {
            if (_contextHelper.IsRequestedContent(originalContent) && _testDataCookieHelper.IsTestParticipant(cookie))
            {
                //increment view if not already done
                if (!cookie.Viewed)
                {
                    _testManager.IncrementCount(cookie.TestId, cookie.TestContentId, contentVersion,
                        CountType.View);
                    cookie.Viewed = true;

                    _testDataCookieHelper.UpdateTestDataCookie(cookie);
                }
            }
        }

        /// <summary>
        /// Analyzes existing cookies and expires / updates any depending on what tests are in the cache.
        /// It is assumed that only tests in the cache are active.
        /// </summary>
        private void EvaluateCookies(ContentEventArgs e)
        {
            var testCookieList = _testDataCookieHelper.GetTestDataFromCookies();
            foreach (var testCookie in testCookieList)
            {
                var activeTest = _testManager.GetActiveTestsByOriginalItemId(testCookie.TestContentId).FirstOrDefault();
                if (activeTest == null)
                {
                    // if cookie exists but there is no associated test, expire it 
                    if (_testDataCookieHelper.HasTestData(testCookie))
                    {
                        _testDataCookieHelper.ExpireTestDataCookie(testCookie);
                    }
                }
                else if (activeTest.Id != testCookie.TestId)
                {
                    // else we have a valid test but the cookie test id doesnt match because user created a new test 
                    // on the same content.
                    _testDataCookieHelper.ExpireTestDataCookie(testCookie);

                    var originalContent = e.Content;
                    var contentVersion = e.ContentLink.WorkID == 0 ? e.ContentLink.ID : e.ContentLink.WorkID;
                    TestDataCookie tc = new TestDataCookie();
                    SetTestData(e.Content, activeTest, tc, contentVersion, out tc, out contentVersion);
                }
            }
        }

        /// <summary>
        /// Processes the Kpis, determining conversions and handling incrementing conversion counts.
        /// </summary>
        /// <param name="e"></param>
        private void EvaluateKpis(ContentEventArgs e)
        {
            // TargetLink is only not null once during all the calls, this optimizes the calls to check for kpi conversions.
            if (e.TargetLink != null)
            {
                var cookielist = _testDataCookieHelper.GetTestDataFromCookies();
                foreach (var tdcookie in cookielist)
                {
                    // for every test cookie we have, check for the converted and the viewed flag
                    if (!tdcookie.Converted && tdcookie.Viewed)
                    {
                        var test = _testManager.GetActiveTestsByOriginalItemId(tdcookie.TestContentId).FirstOrDefault();
                        if (test != null)
                        {
                            // optimization : Evalute only the kpis that have not currently evaluated to true.
                            var kpis = new List<IKpi>();
                            foreach (var kpi in test.KpiInstances)
                            {
                                var converted = tdcookie.KpiConversionDictionary.First(x => x.Key == kpi.Id).Value;
                                if (!converted)
                                {
                                    kpis.Add(kpi);
                                }
                            }

                            var evaluated = _testManager.EvaluateKPIs(kpis, e.Content);
                            if (evaluated.Count > 0)
                            {
                                // add each kpi to testdata cookie data
                                foreach (var eval in evaluated)
                                {
                                    tdcookie.KpiConversionDictionary.Remove(eval);
                                    tdcookie.KpiConversionDictionary.Add(eval, true);
                                    _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.KpiConvertedEvent, new KpiEventArgs(kpis.FirstOrDefault(k => k.Id == eval), test));
                                }

                                // now check to see if all kpi objects have evalated
                                tdcookie.Converted = tdcookie.KpiConversionDictionary.All(x => x.Value);

                                // now save the testdata to the cookie
                                _testDataCookieHelper.UpdateTestDataCookie(tdcookie);

                                // now if we have converted, fire the converted message 
                                // note : we wouldnt be here if we already converted on a previous loop
                                if (tdcookie.Converted)
                                {
                                    Variant varUserSees = test.Variants.First(x => x.Id == tdcookie.TestVariantId);
                                    _testManager.EmitUpdateCount(test.Id, varUserSees.ItemId, varUserSees.ItemVersion,
                                        CountType.Conversion);

                                    _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.AllKpisConvertedEvent, new KpiEventArgs(tdcookie.KpiConversionDictionary, test));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
