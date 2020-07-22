using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web
{
    /// <summary>
    /// Enables and disables AB testing based on number of active tests in cache.
    /// </summary>
    [ServiceConfiguration(ServiceType = typeof(IFeatureEnabler), Lifecycle = ServiceInstanceScope.Singleton)]
    public class FeatureEnabler: IFeatureEnabler
    {
        private IServiceLocator serviceLocator;

        /// <summary>
        /// Default.
        /// </summary>
        /// <param name="serviceLocator"></param>
        public FeatureEnabler(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
            var testingEvents = serviceLocator.GetInstance<IMarketingTestingEvents>();
            testingEvents.TestAddedToCache += TestAddedToCache;
            testingEvents.TestRemovedFromCache += TestRemovedFromCache;
        }

        /// <summary>
        /// Enables AbTesting when the active tests count is one.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TestAddedToCache(object sender, TestEventArgs e)
        {
            var monitor = serviceLocator.GetInstance<IConfigurationMonitor>();
            monitor.HandleConfigurationChange();
        }

        /// <summary>
        /// Disables AbTesting when the active tests count is zero.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void TestRemovedFromCache(object sender, TestEventArgs e)
        {
            var monitor = serviceLocator.GetInstance<IConfigurationMonitor>();
            monitor.HandleConfigurationChange();
        }
    }
}
