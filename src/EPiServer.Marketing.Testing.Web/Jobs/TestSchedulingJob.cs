using EPiServer.BaseLibrary.Scheduling;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Web.Jobs
{

    /// <summary>
    /// Scheduled job class that automatically starts and stops tests
    /// </summary>
    [ScheduledPlugIn(
        DisplayName = "Marketing Test Monitor",
        Description = "Starts and stops tests based on their start and stop dates.",
        SortIndex = 0,              // Brings it to top of job list.
        DefaultEnabled = true,      // By default the task is enabled.
        InitialTime = "00:02:00",   // First time only, start after 2 min
        IntervalLength = 30,        // Default configured interval is 30 minutes
        IntervalType = ScheduledIntervalType.Minutes,
        LanguagePath = "/multivariate/scheduler_plugin")
    ]
    public class TestSchedulingJob : ScheduledJobBase
    {
        private IServiceLocator _locator;

        [ExcludeFromCodeCoverage]
        public TestSchedulingJob()
        {
            _locator = ServiceLocator.Current;
        }

        internal TestSchedulingJob(IServiceLocator locator)
        {
            _locator = locator;
        }

        public override string Execute()
        {
            int started = 0, stopped = 0, active = 0, inactive = 0, done = 0;
            string msg = "Started [{0}] Stopped [{1}] Active [{2}] Inactive [{3}] Completed [{4}]";

            var tm = _locator.GetInstance<ITestManager>();

            // start / stop any tests that are scheduled to start / stop
            foreach (var test in tm.GetTestList(new TestCriteria()))
            {
                if (test.State == TestState.Active)
                {
                    if (DateTime.UtcNow > test.EndDate )
                    { tm.Stop(test.Id); stopped++; }
                }
                else if (test.State == TestState.Inactive)
                {
                    if( DateTime.UtcNow > test.StartDate )
                    { tm.Start(test.Id); started++; }
                }
            }

            // Calculate active, inactive and done for log message
            foreach ( var test in tm.GetTestList(new TestCriteria()) )
            {
                if( test.State == TestState.Active )
                { active++; }
                else if (test.State == TestState.Inactive)
                { inactive++; }
                else if (test.State == TestState.Done)
                { done++; }
            }

            return string.Format(msg, started, stopped, active, inactive, done);
        }
    }
}
