using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Linq;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Data;

namespace EPiServer.Marketing.Testing.Web
{
    internal class TestHandler : ITestHandler
    {
        internal List<ContentReference> ProcessedContentList;
        private readonly TestingContextHelper _contextHelper = new TestingContextHelper();
        private readonly ITestDataCookieHelper _testDataCookieHelper = new TestDataCookieHelper();

        private TestDataCookie _testData;
        private ITestManager _testManager;

        private bool? _swapDisabled;
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
            
        }

        [ExcludeFromCodeCoverage]
        internal TestHandler(ITestManager testManager, ITestDataCookieHelper cookieHelper, List<ContentReference> processedList, bool? isSwapDisabled)
        {
            _testDataCookieHelper = cookieHelper;
            ProcessedContentList = processedList;
            _testManager = testManager;
            SwapDisabled = isSwapDisabled;

        }
        

        [ExcludeFromCodeCoverage]
        public void Initialize()
        {
            _testManager = new TestManager();
            ProcessedContentList = new List<ContentReference>();
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.LoadedContent += LoadedContent;

        }

        private void EvaluateKpis(ContentEventArgs e)
        {
            // Get all the cookies with the prefix "EPI-MAR-" ( name of cookie is prefix + testid )
            // for each cookie
            //      if not conversion flag in cookie
            //          get the test id and the test instance from test manager
            //          for each Kpi in test // to skip evaulating kpis that have already evaluated to true
            //              if this Kpi cookie flag doesnt exist
            //                  add kpi object to KpiList
            //          call testmanager.EvaluateKPI( KpiList, e.ContentGuid )
            //          for each guid in returned list
            //              add kpi guid to cookie
            //          
            //          if all KPI evaluated true (in kpis associated with this test)
            //              update conversion count for test
            //              update conversion bool in cookie
            //
            var cdl = _testDataCookieHelper.getTestDataFromCookies();
            foreach( var testdata in cdl )
            {
                if( !testdata.Converted && testdata.Viewed )
                {
                    var test = _testManager.Get(testdata.TestId);
                    var evaluated = _testManager.EvaluateKPIs(test.KpiInstances, e.Content);
                    if( evaluated.Count > 0 )
                    {
                        // add each kpi to testdata cookie data
                        foreach (var eval in evaluated)
                        {
                            testdata.KpiConversionDictionary.Remove(eval);
                            testdata.KpiConversionDictionary.Add(eval,true);
                        }

                        // now check to see if all kpi objects have evalated
                        // if we find one that did not evaluated, dont convert yet
                        testdata.Converted = true;
                        foreach( var kvp in testdata.KpiConversionDictionary )
                        {
                            if( kvp.Value == false) 
                            {
                                testdata.Converted = false;
                                break;
                            }
                        }
                        _testDataCookieHelper.SaveTestDataToCookie(testdata);

                        // now if we have converted, fire the converted message
                        if(testdata.Converted)
                        {
                            Variant varUserSees = test.Variants[0];
                            foreach (var v in test.Variants)
                            {
                                if(v.Id == testdata.TestVariantId)
                                {
                                    varUserSees = v;
                                }

                            }
                            _testManager.EmitUpdateCount(test.Id, varUserSees.Id, varUserSees.ItemVersion, CountType.Conversion);
                        }
                    }
                }
            }
        }

        public void LoadedContent(object sender, ContentEventArgs e)
        {
            if (!SwapDisabled==true ) // for unit testing...
            {
                if (e.TargetLink != null)
                {
                    EvaluateKpis(e);    // new method to evaluate Kpi
                }

                // existing logic 
                ProcessedContentList.Add(e.ContentLink);

                // finds the test associated with the content about to be rendered 
                var activeTest = _testManager.CreateActiveTestCache().FirstOrDefault(x => x.OriginalItemId == e.Content.ContentGuid);
                if (activeTest != null)
                {
                    _testData = _testDataCookieHelper.GetTestDataFromCookie(e.Content.ContentGuid.ToString());

                    var hasData = _testDataCookieHelper.HasTestData(_testData);
                    if (hasData && _testDataCookieHelper.IsTestParticipant(_testData) && _testData.ShowVariant)
                    {
                        Swap(e);
                    }
                    else if (!hasData && ProcessedContentList.Count == 1)
                    {
                        //get a new random variant. 
                        var newVariant = _testManager.ReturnLandingPage(activeTest.Id);
                        _testData.TestId = activeTest.Id;
                        _testData.TestContentId = activeTest.OriginalItemId;
                        _testData.TestVariantId = newVariant.Id;
                        foreach (var kpi in activeTest.KpiInstances)
                        {
                            _testData.KpiConversionDictionary.Add(kpi.Id, false);
                        }

                        if (newVariant.Id != Guid.Empty)
                        {
                            var contentVersion = e.ContentLink.WorkID == 0 ? e.ContentLink.ID : e.ContentLink.WorkID;

                            if (newVariant.ItemVersion != contentVersion)
                            {
                                contentVersion = newVariant.ItemVersion;
                                _testData.ShowVariant = true;
                                _testDataCookieHelper.SaveTestDataToCookie(_testData);

                                Swap(e);
                            }
                            else
                            {
                                _testData.ShowVariant = false;
                            }

                            CalculateView(contentVersion);
                        }
                    }
                }
            }
        }

        private void Swap(ContentEventArgs activeContent)
        {
            if (_testData.ShowVariant)
            {

                var variant = _testManager.CreateVariantPageDataCache(activeContent.Content.ContentGuid, ProcessedContentList);
                //swap it with the cached version
                if (variant != null)
                {
                    activeContent.ContentLink = variant.ContentLink;
                    activeContent.Content = variant;
                }
            }
        }

        private void CalculateView(int contentVersion)
        {


            //increment view if not already done
            if (_testData.Viewed == false)
            {
                _testManager.IncrementCount(_testData.TestId, _testData.TestVariantId, contentVersion,
                    CountType.View);
            }
            //set viewed = true in testdata
            _testData.Viewed = true;
            _testDataCookieHelper.UpdateTestDataCookie(_testData);
        }

        public void Uninitialize()
        {
        }
    }


}