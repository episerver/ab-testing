using EPiServer.Framework;
using EPiServer.ServiceLocation;
using System;
using EPiServer.Framework.Initialization;
using EPiServer.Web;
using EPiServer.Editor;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Web.Repositories;
using System.Threading;

namespace EPiServer.Marketing.Testing.Web.Initializer
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    class Initializer : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
        }

        public void Initialize(InitializationEngine context)
        {
            context.InitComplete += InitComplete;
            context.Locate.TemplateResolver().TemplateResolved += OnTemplateResolved;
        }

        /// <summary>
        /// Listener that detects when all module initialization is complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitComplete(object sender, EventArgs e)
        {
            // Great place to initialize the queue or other services that are not 
            // automatically instantiated.
        }

        /// <summary>
        /// Listener that allows us to detect when an IContent instance is rendered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTemplateResolved(object sender, TemplateResolverEventArgs e)
        {
            // If in editing mode, we will do nothing.
            if (PageEditing.PageIsInEditMode)
                return;

            if ((e.ItemToRender is IContent)) 
            {
                var item        = e.ItemToRender as IContent;
                var itemGuid    = item.ContentGuid;
                var contentRef  = item.ContentLink;
                var name        = item.Name;
                var selectedTemplate = e.SelectedTemplate;

                // Note that it is possible to change the controller right here if you know where
                // you want to redirect to (e.SelectedTemplate) In a multivarient test do we do it 
                // right here or do we use displaychannels? No idea really but we can at least update
                // view stats
                // I.e. google "episerver TemplateResolved" and Displaychannels 
                // http://world.episerver.com/documentation/Items/Developers-Guide/EPiServer-CMS/8/Rendering/Display-channels/Display-channels/
            }
        }

        public void Uninitialize(InitializationEngine context)
        {
            // Remove the event listeners
            context.InitComplete -= InitComplete;
            ServiceLocator.Current.GetInstance<TemplateResolver>().TemplateResolved -= OnTemplateResolved;
        }
    }
}
