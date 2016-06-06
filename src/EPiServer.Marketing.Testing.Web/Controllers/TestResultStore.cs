using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("MarketingTestingResultStore")]
    public class TestResultStore : RestControllerBase
    {
        private IContentRepository _contentRepository;
        private IMarketingTestingWebRepository _testRepository;

        [ExcludeFromCodeCoverage]
        public TestResultStore()
        {
            var serviceLocator = ServiceLocator.Current;
            _contentRepository = serviceLocator.GetInstance<IContentRepository>();
            _testRepository = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
        }

        internal TestResultStore(IServiceLocator serviceLocator)
        {
            _contentRepository = serviceLocator.GetInstance<IContentRepository>();
            _testRepository = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
        }

        [HttpPost]
        public ActionResult Post(TestResultStoreModel testResult)
        {
            RestStatusCodeResult aResult = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);

            if (!string.IsNullOrEmpty(testResult.WinningContentLink))
            {
                //setup versions as ints for repository
                int workingPublishedVersion, workingVariantVersion;

                int.TryParse(testResult.WinningContentLink.Split('_')[0], out workingPublishedVersion);
                int.TryParse(testResult.WinningContentLink.Split('_')[0], out workingVariantVersion);

                //get current test data and content data for published and variant content
                IMarketingTest currentTest = _testRepository.GetTestById(Guid.Parse(testResult.TestId));
                var draftContent =
                    _contentRepository.Get<ContentData>(ContentReference.Parse(testResult.DraftContentLink))
                        .CreateWritableClone() as IContent;
                var publishedContent =
                    _contentRepository.Get<ContentData>(ContentReference.Parse(testResult.PublishedContentLink))
                        .CreateWritableClone() as IContent;

                try
                {
                    Guid workingVariantId;
                    //publish draft content for history tracking.
                    //Even if winner is the current published version we want to show the draft
                    //had been on the site as published.
                    _contentRepository.Save(draftContent, DataAccess.SaveAction.Publish);

                    if (testResult.WinningContentLink == testResult.PublishedContentLink)
                    {
                        //republish original published version as winner.
                        _contentRepository.Save(publishedContent, DataAccess.SaveAction.Publish);

                        //get the appropriate variant and set IsWinner to True. Archive test to show completion.
                        workingVariantId =
                            currentTest.Variants.FirstOrDefault(x => x.ItemVersion == workingPublishedVersion).Id;

                        
                        _testRepository.ArchiveMarketingTest(currentTest.Id, workingVariantId, workingPublishedVersion);
                        aResult = new RestStatusCodeResult((int)HttpStatusCode.Created);
                        return aResult;
                    }

                    //get the appropriate alternate variant and set IsWinner to True. Archive test to show completion.
                    workingVariantId =
                        currentTest.Variants.FirstOrDefault(x => x.ItemVersion == workingVariantVersion).Id;
                    _testRepository.ArchiveMarketingTest(currentTest.Id, workingVariantId, workingVariantVersion);
                    aResult = new RestStatusCodeResult((int)HttpStatusCode.Created);
                    return aResult;
                }
                catch (Exception ex)
                {
                    return aResult;
                }
            }

            return aResult;
        }

    }
}

