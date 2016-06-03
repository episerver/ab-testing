using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("MarketingTestingResultStore")]
    public class TestResultStore : RestControllerBase
    {
        private IContentRepository _contentRepository;
        private ITestManager _testManager;


        [ExcludeFromCodeCoverage]
        public TestResultStore()
        {
            var serviceLocator = ServiceLocator.Current;
            _contentRepository = serviceLocator.GetInstance<IContentRepository>();
            _testManager = serviceLocator.GetInstance<ITestManager>();

        }

        internal TestResultStore(IServiceLocator serviceLocator)
        {
            _contentRepository = serviceLocator.GetInstance<IContentRepository>();
            _testManager = serviceLocator.GetInstance<ITestManager>();
        }

        [HttpPost]
        public ActionResult Post(TestResultStoreModel testResult)
        {
            ContentReference result;
            RestStatusCodeResult aResult = new RestStatusCodeResult((10));

            if (!string.IsNullOrEmpty(testResult.WinningContentLink))
            {
                try
                {
                    int version;
                    var content = _contentRepository.Get<ContentData>(ContentReference.Parse(testResult.WinningContentLink)).CreateWritableClone() as IContent;
                    if (testResult.WinningContentLink == testResult.PublishedContentLink)
                    {
                        var draftContent =
                            _contentRepository.Get<ContentData>(ContentReference.Parse(testResult.DraftContentLink))
                                .CreateWritableClone() as IContent;
                        result = _contentRepository.Save(draftContent, DataAccess.SaveAction.Publish);
                    }

                    result = _contentRepository.Save(content, DataAccess.SaveAction.Publish);


                    var currentTest = _testManager.Get(Guid.Parse(testResult.TestId));
                    
                    if (content.ContentLink.WorkID == 0)
                    {
                        version = content.ContentLink.ID;
                    }
                    else
                    {
                        version = content.ContentLink.WorkID;
                    }

                    currentTest.Variants.FirstOrDefault(x => x.ItemVersion == version).IsWinner = true;
                    _testManager.Save(currentTest);


                    aResult = new RestStatusCodeResult((int)HttpStatusCode.Created);
                }
                catch (Exception ex)
                {
                    aResult = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                aResult = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            return aResult;
        }

    }
}

