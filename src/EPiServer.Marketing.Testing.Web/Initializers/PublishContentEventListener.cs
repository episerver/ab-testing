using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Repositories;
using System.Diagnostics.CodeAnalysis;
using System;
using System.Collections.Generic;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Testing.Web.Helpers;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    /// <summary>
    /// Used internally to detect publishing events and reject/allow publishing of content that 
    /// is part of a test. If content is the source of an ab test and published by pick a winner screen
    /// its allowed, but if its part of a test and published via cms apis or other mechanisms in the cms
    /// UI it is rejected and the ContentEventArgs contains the error string.
    /// Note that content that is not part of the ab test is always ignored.
    /// 
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(MarketingTestingInitialization))]
    public class PublishContentEventListener : IInitializableModule
    {
        private IServiceLocator _locator;
        private static IList<IContent> _contentList = new List<IContent>();
        private static readonly Object _listLock = new Object();
        private IEpiserverHelper _episerverHelper;

        [ExcludeFromCodeCoverage]
        public PublishContentEventListener()
        {
            //_episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
        }

        internal PublishContentEventListener(IServiceLocator locator, IList<IContent> contentList)
        {
            _locator = locator;
            _contentList = contentList;
            _episerverHelper = locator.GetInstance<IEpiserverHelper>();
        }

        [ExcludeFromCodeCoverage]
        public void Initialize(InitializationEngine context)
        {
            _locator = ServiceLocator.Current;
            var contentEvents = _locator.GetInstance<IContentEvents>();
            contentEvents.PublishingContent += _publishingContentEventHandler;
            contentEvents.CheckingInContent += _checkingInContentEventHandler; // checkin content is how content is scheduled for publish.
        }

        [ExcludeFromCodeCoverage]
        public void Uninitialize(InitializationEngine context)
        {            //Add uninitialization logic           
            var contentEvents = _locator.GetInstance<IContentEvents>();
            contentEvents.PublishingContent -= _publishingContentEventHandler;
            contentEvents.CheckingInContent -= _checkingInContentEventHandler;
        }

        /// <summary>
        /// Called by our web repository to add content to the internal list so
        /// that publish calls are not rejected when we pick a winner. 
        /// </summary>
        /// <param name="content"></param>
        [ExcludeFromCodeCoverage]
        internal static void addPublishingContent(IContent content)
        {
            lock (_listLock)
            {
                _contentList.Add(content);
            }
        }

        public void _checkingInContentEventHandler(object sender, ContentEventArgs e)
        {
            _episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
            var repo = _locator.GetInstance<IMarketingTestingWebRepository>();
            var test = repo.GetActiveTestForContent(e.Content.ContentGuid, _episerverHelper.GetContentCultureinfo());
            if (test.Id != Guid.Empty)
            {
                var c = e.Content as IVersionable;
                if (c.StartPublish > DateTime.Now) // scheduled in the future to be published.
                {
                    e.CancelAction = true;
                    var ls = _locator.GetInstance<LocalizationService>();
                    e.CancelReason = ls.GetString("/abtesting/publishing/error_cannot_schedule_publish");
                }
            }
        }

        public void _publishingContentEventHandler(object sender, ContentEventArgs e)
        {
            _episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
            var repo = _locator.GetInstance<IMarketingTestingWebRepository>();
            var test = repo.GetActiveTestForContent(e.Content.ContentGuid, _episerverHelper.GetContentCultureinfo());
            if( test.Id != Guid.Empty)
            {
                if( _contentList.Contains(e.Content) )
                {
                    // web repo added the content to the list
                    lock (_listLock)
                    {
                        _contentList.Remove(e.Content);
                    }
                }
                else
                {
                    // web repo DID NOT add to the list therefore somebody else is trying to 
                    // publish content associated with a test - cancel it.
                    e.CancelAction = true;
                    var ls = _locator.GetInstance<LocalizationService>();
                    e.CancelReason = ls.GetString("/abtesting/publishing/error_cannot_publish");
                }
            }
        }
    }
}
