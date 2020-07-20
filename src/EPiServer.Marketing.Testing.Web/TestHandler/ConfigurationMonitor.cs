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
        private bool lastState;
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

            testHandler.EnableABTesting();
            this.lastState = true;

            HandleConfigurationChange();
            this.cacheSignal.Monitor(HandleConfigurationChange);
        }

        /// <summary>
        /// Enables or disables AB testing based on config changes.
        /// </summary>
        public void HandleConfigurationChange()
        {
            var allActiveTests = new TestCriteria();
            allActiveTests.AddFilter(
                new ABTestFilter
                {
                    Property = ABTestProperty.State,
                    Operator = FilterOperator.And,
                    Value = TestState.Active
                }
            );
            var dbTests = testManager.GetTestList(allActiveTests);

            AdminConfigTestSettings.Reset();
            bool currentState = AdminConfigTestSettings.Current.IsEnabled && dbTests.Count >= 1;

            if (currentState != lastState)
            {
                if (currentState)
                {
                    testHandler.EnableABTesting();
                }
                else
                {
                    testHandler.DisableABTesting();
                }
                lastState = currentState;
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
