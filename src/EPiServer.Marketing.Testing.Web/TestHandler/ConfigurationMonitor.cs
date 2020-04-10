using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web
{
    /// <summary>
    /// Contains methods to monitor configuration changes and apply them to all the remote nodes.
    /// </summary>
    public class ConfigurationMonitor : IConfigurationMonitor
    {
        private IServiceLocator serviceLocator;

        /// <summary>
        /// Default
        /// </summary>
        /// <param name="serviceLocator"></param>
        public ConfigurationMonitor(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        /// <summary>
        /// Enables or disables AB testing based on config changes.
        /// </summary>
        public void HandleConfigurationChange()
        {
            var testHandler = serviceLocator.GetInstance<ITestHandler>();
            if (AdminConfigTestSettings.Current.IsEnabled)
            {
                testHandler.EnableABTesting();
            }
            else
            {
                testHandler.DisableABTesting();
            }
        }
    }
}
