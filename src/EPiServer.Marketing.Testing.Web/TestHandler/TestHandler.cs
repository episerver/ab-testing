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
using EPiServer.Marketing.Testing.Core.Exceptions;
using EPiServer.Logging;

namespace EPiServer.Marketing.Testing.Web
{
    internal class TestHandler : ITestHandler
    {
        internal List<ContentReference> ProcessedContentList;

        private ITestingContextHelper _contextHelper;
        private ITestDataCookieHelper _testDataCookieHelper;
        private ILogger _logger;
        private ITestManager _testManager;
        private bool? _swapDisabled;

        //allows SwapDisabled to be explicitly set (e.g unit tests)
        //Otherwise set via context helper.
        public bool? SwapDisabled
        {
            set { _swapDisabled = value; }

            get
            {
                if (_swapDisabled == null)
                {
                    return _contextHelper.IsInSystemFolder();

                }
                return _swapDisabled;
            }
        }

        [ExcludeFromCodeCoverage]
        public TestHandler()
        {
            _testDataCookieHelper = new TestDataCookieHelper();
            _contextHelper = new TestingContextHelper();
            _logger = LogManager.GetLogger();
        }

        //To support unit testing
        internal TestHandler(ITestManager testManager, ITestDataCookieHelper cookieHelper, List<ContentReference> processedList, ITestingContextHelper contextHelper, ILogger logger)
        {
            _testDataCookieHelper = cookieHelper;
            ProcessedContentList = processedList;
            _testManager = testManager;
            _contextHelper = contextHelper;
            _logger = logger;
        }

        [ExcludeFromCodeCoverage]
        public void Initialize()
        {
            _testManager = new TestManager();
            ProcessedContentList = new List<ContentReference>();
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.LoadedContent += LoadedContent;
            contentEvents.DeletedContent += ContentEventsOnDeletedContent;
            contentEvents.DeletingContentVersion += ContentEventsOnDeletingContentVersion;
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

        /// Main worker method.  Processes each content which triggers a
        /// content loaded event to determine the state of a test and what content to display.
        public void LoadedContent(object sender, ContentEventArgs e)
        {
            if (!SwapDisabled == true)
            {
                try
                {
                    IContent currentPage = _contextHelper.GetCurrentPageFromUrl();

                    if (e.TargetLink != null)
                    {
                        EvaluateKpis(e);    // new method to evaluate Kpi
                    }

                    // Causing numerous errors at startup
                    if (e.Content == null)
                        return;

                    var activeTest = _testManager.GetActiveTestsByOriginalItemId(e.Content.ContentGuid).FirstOrDefault();
                    var testCookieData = _testDataCookieHelper.GetTestDataFromCookie(e.Content.ContentGuid.ToString());
                    var hasData = _testDataCookieHelper.HasTestData(testCookieData);

                    if (activeTest != null)
                    {
                        if (hasData && _testDataCookieHelper.IsTestParticipant(testCookieData) && testCookieData.ShowVariant)
                        {
                            ProcessedContentList.Add(e.ContentLink);
                            Swap(testCookieData, e);
                        }
                        else if (!hasData && ProcessedContentList.Count == 0)
                        {
                            ProcessedContentList.Add(e.ContentLink);
                            //get a new random variant. 
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
                                var contentVersion = e.ContentLink.WorkID == 0 ? e.ContentLink.ID : e.ContentLink.WorkID;

                                if (newVariant.ItemVersion != contentVersion)
                                {
                                    contentVersion = newVariant.ItemVersion;
                                    testCookieData.ShowVariant = true;
                                    _testDataCookieHelper.SaveTestDataToCookie(testCookieData);

                                    Swap(testCookieData, e);
                                }
                                else
                                {
                                    testCookieData.ShowVariant = false;
                                }

                                CalculateView(testCookieData, contentVersion);
                            }
                            else
                            {
                                _testDataCookieHelper.SaveTestDataToCookie(testCookieData);
                            }
                        }
                    }
                    else if (hasData)
                    {
                        _testDataCookieHelper.ExpireTestDataCookie(testCookieData);
                    }
                }
                catch (Exception err)
                {
                    _logger.Error("TestHandler", err);
                }
            }
        }

        //Handles the swapping of content data
        private void Swap(TestDataCookie cookie, ContentEventArgs activeContent)
        {
            if (cookie.ShowVariant)
            {
                var variant = _testManager.GetVariantContent(activeContent.Content.ContentGuid, ProcessedContentList);
                //swap it with the cached version
                if (variant != null)
                {
                    activeContent.ContentLink = variant.ContentLink;
                    activeContent.Content = variant;
                }
            }
        }

        //Handles the incrementing of view counts on a version
        private void CalculateView(TestDataCookie cookie, int contentVersion)
        {
            //increment view if not already done
            if (cookie.Viewed == false)
            {
                _testManager.IncrementCount(cookie.TestId, cookie.TestContentId, contentVersion,
                    CountType.View);
            }
            //set viewed = true in testdata
            cookie.Viewed = true;
            _testDataCookieHelper.UpdateTestDataCookie(cookie);
        }

        //Processes the Kpis, determining conversions and handling incrementing conversion counts.
        private void EvaluateKpis(ContentEventArgs e)
        {
            var cdl = _testDataCookieHelper.getTestDataFromCookies();
            foreach (var testdata in cdl)
            {
                // for every test cookie we have, check for the converted and the viewed flag
                if (!testdata.Converted && testdata.Viewed)
                {
                    try
                    {
                        var test = _testManager.Get(testdata.TestId);

                        // optimization : create the list of kpis that have not evaluated 
                        // to true and then evaluate them
                        var kpis = new List<IKpi>();
                        foreach (var kpi in test.KpiInstances)
                        {
                            var converted = testdata.KpiConversionDictionary.First(x => x.Key == kpi.Id).Value;
                            if (!converted)
                                kpis.Add(kpi);
                        }

                        var evaluated = _testManager.EvaluateKPIs(kpis, e.Content);
                        if (evaluated.Count > 0)
                        {
                            // add each kpi to testdata cookie data
                            foreach (var eval in evaluated)
                            {
                                testdata.KpiConversionDictionary.Remove(eval);
                                testdata.KpiConversionDictionary.Add(eval, true);
                            }

                            // now check to see if all kpi objects have evalated
                            testdata.Converted = !testdata.KpiConversionDictionary.Where(x => x.Value == false).Any();

                            // now save the testdata to the cookie
                            _testDataCookieHelper.SaveTestDataToCookie(testdata);

                            // now if we have converted, fire the converted message 
                            // note : we wouldnt be here if we already converted on a previous loop
                            if (testdata.Converted)
                            {
                                Variant varUserSees = test.Variants.First(x => x.Id == testdata.TestVariantId);
                                _testManager.EmitUpdateCount(test.Id, varUserSees.ItemId, varUserSees.ItemVersion, CountType.Conversion);
                            }
                        }
                    }
                    catch (TestNotFoundException)
                    {
                        _testDataCookieHelper.ExpireTestDataCookie(testdata);
                    }
                }
            }
        }
    }
}
