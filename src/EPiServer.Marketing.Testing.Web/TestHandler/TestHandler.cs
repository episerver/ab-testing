using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using System.Linq;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;

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
        internal TestHandler(ITestManager testManager, ITestDataCookieHelper cookieHelper, List<ContentReference> processedList)
        {
            _testDataCookieHelper = cookieHelper;
            ProcessedContentList = processedList;
            _testManager = testManager;
        }

        [ExcludeFromCodeCoverage]
        public void Initialize()
        {
            _testManager = new TestManager();
            ProcessedContentList = new List<ContentReference>();
            var contentEvents = ServiceLocator.Current.GetInstance<IContentEvents>();
            contentEvents.LoadedContent += LoadedContent;
        }

        public void LoadedContent(object sender, ContentEventArgs e)
        {
            if (!SwapDisabled == true)
            {
                var activeTest =
                    _testManager.CreateActiveTestCache().FirstOrDefault(x => x.OriginalItemId == e.Content.ContentGuid);

                _testData = _testDataCookieHelper.GetTestDataFromCookie(e.Content.ContentGuid.ToString());
                var hasData = _testDataCookieHelper.HasTestData(_testData);

                if (activeTest != null)
                {
                    ProcessedContentList.Add(e.ContentLink);
                    
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
                else if(hasData)
                {
                    _testDataCookieHelper.ExpireTestDataCookie(_testData);
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