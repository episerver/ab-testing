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
using System.Linq;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Core.Statistics;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;

namespace EPiServer.Marketing.Testing.Web.Jobs
{

    /// <summary>
    /// Scheduled job class that automatically starts and stops tests
    /// </summary>
    [ScheduledPlugIn(
        DisplayName = "displayname",
        Description = "description",
        SortIndex = 0,              // Brings it to top of job list.
        DefaultEnabled = true,      // By default the task is enabled.
        InitialTime = "00:02:00",   // First time only, start after 2 min
        IntervalLength = 30,        // Default configured interval is 30 minutes
        IntervalType = ScheduledIntervalType.Minutes,
        LanguagePath = "/abtesting/scheduler_plugin")
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
            var ls = _locator.GetInstance<LocalizationService>();
            var msg = ls.GetString("/abtesting/scheduler_plugin/message");

            var tm = _locator.GetInstance<IMarketingTestingWebRepository>();
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
                        var utcEndDate = test.EndDate;
                        if (DateTime.UtcNow > utcEndDate) // stop it now
                        {
                            tm.StopMarketingTest(test.Id);

                            if (test.AutoPublishWinner)
                            {
                                // need to get updated test so that we know which variant won since we need to autopublish it
                                var updatedTest = tm.GetTestById(test.Id);
                                var contextHelper = new TestingContextHelper();
                                var contextData = contextHelper.GenerateContextData(updatedTest);
                                var winningLink = updatedTest.Variants.First(v => v.IsWinner).IsPublished ? contextData.PublishedVersionContentLink : contextData.DraftVersionContentLink;

                                var storeModel = new TestResultStoreModel()
                                {
                                    DraftContentLink = contextData.DraftVersionContentLink,
                                    PublishedContentLink = contextData.PublishedVersionContentLink,
                                    TestId = updatedTest.Id.ToString(),
                                    WinningContentLink = winningLink
                                };

                                tm.PublishWinningVariant(storeModel);
                            }

                            stopped++;
                        }
                        else if(NextExecutionUTC > utcEndDate)
                        {
                            // set a newer date to run the job again
                            NextExecutionUTC = utcEndDate;
                        }
                        break;
                    case TestState.Inactive:
                        var utcStartDate = test.StartDate;
                        if ( DateTime.UtcNow > utcStartDate) // start it now
                        {
                            tm.StartMarketingTest(test.Id);
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
