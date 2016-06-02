using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Web.Mvc;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    [RestStore("MarketingTestingResultStore")]
    public class TestResultStore : RestControllerBase
    {
        private IContentRepository _contentRepository;


        [ExcludeFromCodeCoverage]
        public TestResultStore()
        {
            var serviceLocator = ServiceLocator.Current;
            _contentRepository = serviceLocator.GetInstance<IContentRepository>();

        }

        internal TestResultStore(IServiceLocator serviceLocator)
        {
            _contentRepository = serviceLocator.GetInstance<IContentRepository>();
        }

        [HttpPost]
        public ActionResult Post(TestResultStoreModel testResult)
        {
            RestStatusCodeResult aResult = null;

            if (!string.IsNullOrEmpty(testResult.WinningContentLink))
            {
                try
                {
                    int version;
                    var content = _contentRepository.Get<ContentData>(ContentReference.Parse(testResult.WinningContentLink)).CreateWritableClone() as IContent;
                    var publishedGuid =
                        _contentRepository.Get<IContent>(ContentReference.Parse(testResult.PublishedContentLink));
                    var draftGuid = 
                        _contentRepository.Get<IContent>(ContentReference.Parse(testResult.DraftContentLink));

                    if (content.ContentLink.WorkID == 0)
                    {
                        version = content.ContentLink.ID;
                    }
                    else
                    {
                        version = content.ContentLink.WorkID;
                    }





                    var result = _contentRepository.Save(content, DataAccess.SaveAction.Publish);
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

