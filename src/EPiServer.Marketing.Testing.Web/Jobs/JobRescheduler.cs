using EPiServer.Framework;
using System;
using EPiServer.Framework.Initialization;
using System.Diagnostics.CodeAnalysis;
using EPiServer.ServiceLocation;
using EPiServer.DataAbstraction;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Web.Initializers;

namespace EPiServer.Marketing.Testing.Web.Jobs
{
    /// <summary>
    /// Listens for TestSaved and updates the next job run 
    /// if the test is marked to start before the next job run
    /// </summary>
    [InitializableModule]
    [ModuleDependency(typeof(MarketingTestingInitialization))]
    public class JobRescheduler : IInitializableModule
    {
        private IServiceLocator _serviceLocator;

        [ExcludeFromCodeCoverage]
        internal JobRescheduler(IServiceLocator locator)
        {
            _serviceLocator = locator;
        }

        [ExcludeFromCodeCoverage]
        public JobRescheduler()
        {
            // requried default constructor else cms complains when instantiating this class.
        }

        [ExcludeFromCodeCoverage]
        public void Initialize(InitializationEngine context)
        {
            _serviceLocator = context.Locate.Advanced;
            var marketingtestingEvents = _serviceLocator.GetInstance<IMarketingTestingEvents>();
            marketingtestingEvents.TestSaved += OnTestSaved;
        }

        [ExcludeFromCodeCoverage]
        public void Uninitialize(InitializationEngine context)
        {
            var marketingtestingEvents = _serviceLocator.GetInstance<IMarketingTestingEvents>();
            marketingtestingEvents.TestSaved -= OnTestSaved;
        }

        public void OnTestSaved(object sender, TestEventArgs e)
        {
            var repo = _serviceLocator.GetInstance<IScheduledJobRepository>();
            var job = repo.Get("Execute", "EPiServer.Marketing.Testing.Web.Jobs.TestSchedulingJob", "EPiServer.Marketing.Testing.Web");
            if (job.NextExecution > e.Test.StartDate)
            {
                job.NextExecution = e.Test.StartDate; // NextExecution has to be local time
                repo.Save(job);
            }
        }
    }
}
