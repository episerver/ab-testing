using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Linq;
using Castle.Core.Internal;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Logging;
using System.Web;
using EPiServer.Marketing.KPI.Common.Attributes;
using System.Reflection;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Data;

namespace EPiServer.Marketing.Testing.Web
{
    internal class TestHandler : ITestHandler
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly ITestingContextHelper _contextHelper;
        private readonly ITestDataCookieHelper _testDataCookieHelper;
        private readonly ILogger _logger;
        private readonly ITestManager _testManager;
        private readonly DefaultMarketingTestingEvents _marketingTestingEvents;
        /// Used to keep track of how many times for the same service/event we add the proxy event handler
        private readonly IReferenceCounter _ReferenceCounter = new ReferenceCounter();

        /// <summary>
        /// HTTPContext flag used to skip AB Test Processing in LoadContent event handler.
        /// </summary>
        public const string ABTestHandlerSkipFlag = "ABTestHandlerSkipFlag";
        public const string SkipRaiseContentSwitchEvent = "SkipRaiseContentSwitchEvent";
        public const string ABTestHandlerSkipKpiEval = "ABTestHandlerSkipKpiEval";

        [ExcludeFromCodeCoverage]
        public TestHandler()
        {
            _serviceLocator = ServiceLocator.Current;

            _testDataCookieHelper = new TestDataCookieHelper();
            _contextHelper = new TestingContextHelper();
            _logger = LogManager.GetLogger();

            _testManager = _serviceLocator.GetInstance<ITestManager>();
            _marketingTestingEvents = _serviceLocator.GetInstance<DefaultMarketingTestingEvents>();

            // Setup our content events
            var contentEvents = _serviceLocator.GetInstance<IContentEvents>();
            contentEvents.LoadedChildren += LoadedChildren;
            contentEvents.LoadedContent += LoadedContent;
            contentEvents.DeletedContent += ContentEventsOnDeletedContent;
            contentEvents.DeletingContentVersion += ContentEventsOnDeletingContentVersion;

            initProxyEventHandler();
        }

        //To support unit testing
        internal TestHandler(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;

            _testDataCookieHelper = serviceLocator.GetInstance<ITestDataCookieHelper>();
            _contextHelper = serviceLocator.GetInstance<ITestingContextHelper>();
            _logger = serviceLocator.GetInstance<ILogger>();
            _testManager = serviceLocator.GetInstance<ITestManager>();
            _marketingTestingEvents = serviceLocator.GetInstance<DefaultMarketingTestingEvents>();

            IReferenceCounter rc = serviceLocator.GetInstance<IReferenceCounter>();
            _ReferenceCounter = rc;
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
            var repo = _serviceLocator.GetInstance<IContentRepository>();

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

                    // get the test from the cache
                    var activeTest = _testManager.GetActiveTestsByOriginalItemId(e.Content.ContentGuid).FirstOrDefault();
                    if (activeTest != null)
                    {
                        var testCookieData = _testDataCookieHelper.GetTestDataFromCookie(e.Content.ContentGuid.ToString());
                        var hasData = _testDataCookieHelper.HasTestData(testCookieData);
                        var originalContent = e.Content;
                        var contentVersion = e.ContentLink.WorkID == 0 ? activeTest.Variants.First(variant => variant.IsPublished).ItemVersion : e.ContentLink.WorkID;

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
                if (!newVariant.IsPublished)
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

                    //The SkipRaiseContentSwitchEvent flag is necessary in order to only raise our ContentSwitchedEvent
                    //once per content per request.  We save an item of activecontent+flag because we may have multiple 
                    //content items per request which will need to be handled.
                    if (!HttpContext.Current.Items.Contains(activeContent + SkipRaiseContentSwitchEvent))
                    {
                        _marketingTestingEvents.RaiseMarketingTestingEvent(
                            DefaultMarketingTestingEvents.ContentSwitchedEvent,
                            new TestEventArgs(activeTest, activeContent.Content));
                        HttpContext.Current.Items[activeContent + SkipRaiseContentSwitchEvent] = true;
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
                if (!cookie.Viewed && DbReadWrite())
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
            if (!DbReadWrite())
            {
                return;
            }

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
        private void EvaluateKpis(EventArgs e)
        {
            // We only want to evaluate Kpis one time per request.
            if (HttpContext.Current.Items.Contains(ABTestHandlerSkipKpiEval))
            {
                return;
            }

            HttpContext.Current.Items[ABTestHandlerSkipKpiEval] = true;

            var cookielist = _testDataCookieHelper.GetTestDataFromCookies();
            foreach (var tdcookie in cookielist)
            {
                // for every test cookie we have, check for the converted and the viewed flag
                if (tdcookie.Converted || !tdcookie.Viewed)
                {
                    continue;
                }

                var test = _testManager.GetActiveTestsByOriginalItemId(tdcookie.TestContentId).FirstOrDefault();
                if (test == null)
                {
                    continue;
                }

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

                var kpiResults = _testManager.EvaluateKPIs(kpis, e);

                var conversionResults = kpiResults.OfType<KpiConversionResult>();
                ProcessKpiConversionResults(tdcookie, test, kpis, conversionResults);

                var financialResults = kpiResults.OfType<KpiFinancialResult>();
                ProcessKeyFinancialResults(tdcookie, test, financialResults);

                var valueResults = kpiResults.OfType<KpiValueResult>();
                ProcessKeyValueResults(tdcookie, test, valueResults);
            }
        }

        /// <summary>
        /// Loop through conversion results to see if any have converted, if so update views/conversions as necessary
        /// </summary>
        /// <param name="tdcookie"></param>
        /// <param name="test"></param>
        /// <param name="kpis"></param>
        /// <param name="results"></param>
        private void ProcessKpiConversionResults(TestDataCookie tdcookie, IMarketingTest test, List<IKpi> kpis,
            IEnumerable<KpiConversionResult> results)
        {
            // add each kpi to testdata cookie data
            foreach (var result in results)
            {
                if (!result.HasConverted)
                {
                    continue;
                }

                tdcookie.KpiConversionDictionary.Remove(result.KpiId);
                tdcookie.KpiConversionDictionary.Add(result.KpiId, true);
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.KpiConvertedEvent,
                    new KpiEventArgs(kpis.FirstOrDefault(k => k.Id == result.KpiId), test));
            }

            // now check to see if all kpi objects have evalated
            tdcookie.Converted = tdcookie.KpiConversionDictionary.All(x => x.Value);

            // now save the testdata to the cookie
            _testDataCookieHelper.UpdateTestDataCookie(tdcookie);

            // now if we have converted, fire the converted message 
            // note : we wouldnt be here if we already converted on a previous loop
            if (!tdcookie.Converted)
            {
                return;
            }

            var varUserSees = test.Variants.First(x => x.Id == tdcookie.TestVariantId);
            _testManager.EmitUpdateCount(test.Id, varUserSees.ItemId, varUserSees.ItemVersion, CountType.Conversion);

            _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.AllKpisConvertedEvent,
                new KpiEventArgs(tdcookie.KpiConversionDictionary, test));
        }

        private void ProcessKeyFinancialResults(TestDataCookie tdcookie, IMarketingTest test, IEnumerable<KpiFinancialResult> results)
        {
            var varUserSees = test.Variants.First(x => x.Id == tdcookie.TestVariantId);

            foreach (var kpiFinancialResult in results)
            {
                var keyFinancialResult = new KeyFinancialResult()
                {
                    Id = Guid.NewGuid(),
                    KpiId = kpiFinancialResult.KpiId,
                    Total = kpiFinancialResult.Total,
                    VariantId = varUserSees.ItemId,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                //_testManager.AddKpiResultData(test.Id, varUserSees.ItemId, varUserSees.ItemVersion, keyFinancialResult, 0);
                _testManager.EmitKpiResultData(test.Id, varUserSees.ItemId, varUserSees.ItemVersion, keyFinancialResult, 0);
            }
        }

        private void ProcessKeyValueResults(TestDataCookie tdcookie, IMarketingTest test, IEnumerable<KpiValueResult> results)
        {
            var varUserSees = test.Variants.First(x => x.Id == tdcookie.TestVariantId);

            foreach (var kpiValueResult in results)
            {
                var keyValueResult = new KeyValueResult()
                {
                    Id = Guid.NewGuid(),
                    KpiId = kpiValueResult.KpiId,
                    Value = kpiValueResult.Value,
                    VariantId = varUserSees.ItemId,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };

                //_testManager.EmitKpiResultData(test.Id, varUserSees.ItemId, varUserSees.ItemVersion, keyValueResult, 1);
                _testManager.AddKpiResultData(test.Id, varUserSees.ItemId, varUserSees.ItemVersion, keyValueResult, KeyResultType.Value);
            }
        }

        private bool DbReadWrite()
        {
            var dbmode = _serviceLocator.GetInstance<IDatabaseMode>().DatabaseMode;
            return dbmode == DatabaseMode.ReadWrite;
        }

        #region ProxyEventHandlerSupport

        /// <summary>
        /// Handles KPI evaluation logic for KPIs that are triggered from an event.  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ProxyEventHandler(object sender, EventArgs e)
        {
            if (!_contextHelper.SwapDisabled(e) && DbReadWrite())
            {
                try
                {
                    EvaluateKpis(e);
                }
                catch (Exception err)
                {
                    _logger.Error("TestHandler.ProxyEventHandler", err);
                }
            }
        }

        /// <summary>
        /// At startup, initializes all the ProxyEventHandler's for all Kpi objects found in all active tests.
        /// </summary>
        internal void initProxyEventHandler()
        {
            foreach (var test in _testManager.ActiveCachedTests)
            {
                foreach (var kpi in test.KpiInstances)
                {
                    AddProxyEventHandler(kpi);
                }
            }

            // Setup our listener so when tests are added and removed and update our proxyEventHandler
            var e = _serviceLocator.GetInstance<IMarketingTestingEvents>();
            e.TestAddedToCache += TestAddedToCache;
            e.TestRemovedFromCache += TestRemovedFromCache;
        }

        /// <summary>
        /// When a test is added to the active cache, this method will be fired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TestAddedToCache(object sender, TestEventArgs e)
        {
            foreach (var kpi in e.Test.KpiInstances)
            {
                AddProxyEventHandler(kpi);
            }
        }

        /// <summary>
        /// When a test is removed to the active cache, this method will be fired.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TestRemovedFromCache(object sender, TestEventArgs e)
        {
            foreach (var kpi in e.Test.KpiInstances)
            {
                RemoveProxyEventHandler(kpi);
            }
        }

        /// <summary>
        /// Adds the ProxyEventHandler for the given Kpi instance if it supports the EventSpecificationAttribute.
        /// </summary>
        /// <param name="kpi"></param>
        internal void AddProxyEventHandler(IKpi kpi)
        {
            // Add the proxyeventhandler only once, if its in our reference counter, just increment
            // the reference.
            if (!_ReferenceCounter.hasReference(kpi.GetType()))
            {

                kpi.EvaluateProxyEvent += ProxyEventHandler;

                _ReferenceCounter.AddReference(kpi.GetType());
            }
            else
            {
                _ReferenceCounter.AddReference(kpi.GetType());
            }
        }

        /// <summary>
        /// Removes the ProxyEventHandler for the given Kpi instance if it supports the EventSpecificationAttribute.
        /// </summary>
        /// <param name="kpi"></param>
        internal void RemoveProxyEventHandler(IKpi kpi)
        {
            _ReferenceCounter.RemoveReference(kpi.GetType());

            // Remove the proxyeventhandler only once, when the last reference is removed.
            if (!_ReferenceCounter.hasReference(kpi.GetType()))
            {
                kpi.EvaluateProxyEvent -= ProxyEventHandler;
            }
        }
        #endregion
    }
}
