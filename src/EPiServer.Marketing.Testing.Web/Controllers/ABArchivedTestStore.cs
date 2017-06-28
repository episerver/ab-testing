using System;
using System.Web.Mvc;
using System.Diagnostics.CodeAnalysis;

using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Services.Rest;
using System.Net;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Web.Helpers;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    /// <summary>
    /// This is the main RestSTore the UI compontents use to manage IMarketingTestObjects 
    /// </summary>
    [RestStore("ABArchivedTestStore")]
    public class ABArchivedTestStore : RestControllerBase
    {
        private IMarketingTestingWebRepository _webRepo;
        private IEpiserverHelper _episerverHelper;
        private IContentRepository _contentRepo;
        private ILogger _logger;

        [ExcludeFromCodeCoverage]
        public ABArchivedTestStore()
        {
            _webRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
            _contentRepo = ServiceLocator.Current.GetInstance<IContentRepository>();
            _logger = LogManager.GetLogger();
        }

        // For unit test support.
        internal ABArchivedTestStore(IServiceLocator serviceLocator)
        {
            _webRepo = serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            _episerverHelper = serviceLocator.GetInstance<IEpiserverHelper>();
            _contentRepo = serviceLocator.GetInstance<IContentRepository>();
            _logger = serviceLocator.GetInstance<ILogger>();
        }

        /// <summary>
        /// Given the CMS content Guid, returns JSON encoded test object
        /// </summary>
        /// <param name="id">Guid of the cms content</param>
        /// <returns>IMarketingTest object in a Rest result or InternalServerError</returns>
        [HttpGet]
        public ActionResult Get(string id)
        {
            ActionResult result;
            try
            {
                IContent content = _contentRepo.Get<IContent>(new ContentReference(id));

                var cGuid = content.ContentGuid;
                var criteria = new TestCriteria();
                criteria.AddFilter(new ABTestFilter()
                {
                    Property = ABTestProperty.State,
                    Operator = FilterOperator.And,
                    Value = TestState.Archived
                }
                );
                criteria.AddFilter(new ABTestFilter()
                {
                    Property = ABTestProperty.OriginalItemId,
                    Operator = FilterOperator.And,
                    Value = cGuid
                }
                );
                var tests = _webRepo.GetTestList(criteria, _episerverHelper.GetContentCultureinfo());

                result = Rest(tests);
            }
            catch (Exception e)
            {
                _logger.Error("Internal error getting test using content Guid : "
                    + id, e);
                result = new RestStatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            return result;
        }
    }
}
