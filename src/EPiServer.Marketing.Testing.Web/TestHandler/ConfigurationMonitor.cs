using EPiServer.Framework.Cache;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
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
        private ICacheSignal cacheSignal;
        private ITestHandler testHandler;
        private ITestManager testManager;

        /// <summary>
        /// Default
        /// </summary>
        /// <param name="serviceLocator"></param>
        /// <param name="cacheSignal"></param>
        /// 
        public ConfigurationMonitor(IServiceLocator serviceLocator, ICacheSignal cacheSignal)
        {
            this.serviceLocator = serviceLocator;
            this.cacheSignal = cacheSignal;

            testManager = serviceLocator.GetInstance<ITestManager>();
            testHandler = serviceLocator.GetInstance<ITestHandler>();

             this.cacheSignal.Monitor(HandleConfigurationChange);
        }

        /// <summary>
        /// Enables or disables AB testing based on config changes.
        /// </summary>
        public void HandleConfigurationChange()
        {
            var logger = LogManager.GetLogger();

            var testCriteria = new TestCriteria();
            testCriteria.AddFilter(
                new ABTestFilter
                {
                    Property = ABTestProperty.State,
                    Operator = FilterOperator.And,
                    Value = TestState.Active
                }
            );
            var dbTests = testManager.GetTestList(testCriteria);
            var cache = serviceLocator.GetInstance<ISynchronizedObjectInstanceCache>();

            // forces a reload of the config component, not sure we still need to do this.
            AdminConfigTestSettings.Reset(); 
            if(AdminConfigTestSettings.Current.IsEnabled)
            {
                if (dbTests.Count == 0)
                {
                    testHandler.DisableABTesting();
                    cache.Insert("abconfigenabled", "false", new CacheEvictionPolicy(null, null));
                }
                else
                {
                    testHandler.EnableABTesting();
                    cache.Insert("abconfigenabled", "true", new CacheEvictionPolicy(null, null));
                }
            }
            else
            {
                testHandler.DisableABTesting();
                cache.Insert("abconfigenabled", "false", new CacheEvictionPolicy(null, null));
            }

            this.cacheSignal.Set();
        }

        /// <summary>
        /// Called by the UI to reset the monitor and force all other nodes to re-read the config.
        /// </summary>
        public void Reset()
        {
            this.cacheSignal.Reset();
        }
    }
}
