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
using EPiServer.Marketing.Testing.Core.Statistics;

namespace EPiServer.Marketing.Testing.Web.Jobs
{

    /// <summary>
    /// Scheduled job class that automatically starts and stops tests
    /// </summary>
    [ScheduledPlugIn(
        DisplayName = "Marketing Test Monitor",
        Description = "Scans the list of pending or active Marketing tests and changes the test state based on the Start Date and End Date for the test",
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
            var repo = _locator.GetInstance<IScheduledJobRepository>();
            ScheduledJob job = repo.Get(this.ScheduledJobId);
            DateTime NextExecutionUTC = job.NextExecutionUTC;

            // Start / stop any tests that need to be.
            // If any tests are scheduled to start or stop prior to the next scheduled
            // exection date of this job, change the next execution date approprately. 
            foreach (var test in tm.GetTestList(new TestCriteria()))
            {
                switch (test.State)
                {
                    case TestState.Active:
                        var utcEndDate = ((DateTime)test.EndDate).ToUniversalTime();
                        if (DateTime.UtcNow > utcEndDate) // stop it now
                        {
                            tm.Stop(test.Id);
                            stopped++;
                        }
                        else if(NextExecutionUTC > utcEndDate)
                        {
                            // set a newer date to run the job again
                            NextExecutionUTC = utcEndDate;
                        }
                        break;
                    case TestState.Inactive:
                        var utcStartDate = test.StartDate.ToUniversalTime();
                        if ( DateTime.UtcNow > utcStartDate) // start it now
                        {
                            tm.Start(test.Id);
                            started++;
                        }
                        else if (NextExecutionUTC > utcStartDate)
                        {
                            // set a newer date to run the job again
                            NextExecutionUTC = utcStartDate;
                        }
                        break;
                }
            }

            // update the next run time if we need to
            if( job.NextExecutionUTC != NextExecutionUTC )
            {
                if (job.IsEnabled)
                {
                    // NextExecution requires local time
                    job.NextExecution = NextExecutionUTC.ToLocalTime();
                    repo.Save(job);
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
