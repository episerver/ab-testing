using System;
using System.Web.Mvc;
using System.Diagnostics.CodeAnalysis;

using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using System.Net;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Globalization;
using EPiServer.Marketing.Testing.Web.Helpers;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    /// <summary>
    /// This is the main RestStore the UI components use to manage IMarketingTestObjects.
    /// </summary>
    [RestStore("ABTestStore")]
    public class ABTestStore : RestControllerBase
    {
        private IMarketingTestingWebRepository _webRepo;
        private ILogger _logger;
        private IEpiserverHelper _episerverHelper;

        [ExcludeFromCodeCoverage]
        public ABTestStore()
        {
            _webRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
            _logger = LogManager.GetLogger();
        }

        // For unit test support.
        internal ABTestStore(IServiceLocator serviceLocator)
        {
            _webRepo = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            _episerverHelper = serviceLocator.GetInstance<IEpiserverHelper>();
            _logger = serviceLocator.GetInstance<ILogger>();
        }

        /// <summary>
        /// Given the CMS content Guid, returns JSON encoded test object.
        /// </summary>
        /// <param name="id">Guid of the CMS content.</param>
        /// <returns>IMarketingTest object in a Rest result or InternalServerError.</returns>
        [HttpGet]
        public ActionResult Get(string id)
        {
            ActionResult result;
            try
            {
                var cGuid = Guid.Parse(id);
                var aTest = _webRepo.GetActiveTestForContent(cGuid);
                result = Rest(aTest);
            }
            catch(Exception e)
            {
                _logger.Error("Internal error getting test using content Guid : " 
                    + id, e);
                result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }

            return result;
        }

        /// <summary>
        /// Given the CMS content Guid, deletes the specified IMarketingTest (if found).
        /// </summary>
        /// <param name="id">Guid of the CMS content.</param>
        /// <returns>HttpStatusCode.OK or InternalServerError</returns>
        [HttpDelete]
        public ActionResult Delete(string id)
        {
            ActionResult result;
            try
            {
                var cGuid = Guid.Parse(id);
                _webRepo.DeleteTestForContent(cGuid);
                result = new RestStatusCodeResult((int)HttpStatusCode.OK);
            }
            catch(Exception e)
            {
                _logger.Error("Internal error deleting test using content Guid : "
                    + id, e);
                result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            return result;
        }

        /// <summary>
        /// Creates a new test using the supplied TestingStoreModel.
        /// </summary>
        /// <param name="testData"></param>
        /// <returns>HttpStatusCode.Created or HttpStatusCode.InternalServerError</returns>
        [HttpPost]
        public ActionResult Post(TestingStoreModel testData)
        {
            ActionResult result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);

            testData.ContentCulture = _episerverHelper.GetContentCultureinfo();

            try
            {
                if (_webRepo.CreateMarketingTest(testData) != Guid.Empty)
                {
                    result = new RestStatusCodeResult((int)HttpStatusCode.Created);
                }
                else
                {
                    _logger.Error("Internal error creating test, repo did not return a valid Guid : ");
                }

            }
            catch (Exception e)
            {
                _logger.Error("Internal error creating test " + testData.TestTitle, e);
            }

            return result;
        }

        /// <summary>
        /// Given a TestResultStoreModel, publishes the winning variant.
        /// </summary>
        /// <param name="testResult"></param>
        /// <returns>publishedVersionContentLink or InternalServerError</returns>
        [HttpPut]
        public ActionResult Put(TestResultStoreModel testResult)
        {
            ActionResult result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);

            try
            {
                string publishedVersionContentLink = _webRepo.PublishWinningVariant(testResult);
                if (string.IsNullOrEmpty(publishedVersionContentLink))
                {
                    _logger.Error("Failed to publish winning variant, content link retured is null");
                }
                else
                {
                    result = Rest(publishedVersionContentLink);
                }
            }
            catch (Exception e)
            {
                _logger.Error("Failed to publish winning variant", e);
            }

            return result;

        }
    }
}
