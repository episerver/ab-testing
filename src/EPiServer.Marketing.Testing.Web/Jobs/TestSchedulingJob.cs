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
using EPiServer.Marketing.Testing.Web.Statistics;
using EPiServer.Marketing.Testing.Web.Config;
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
            var testingContextHelper = _locator.GetInstance<ITestingContextHelper>();
            var webRepo = _locator.GetInstance<IMarketingTestingWebRepository>();
            var jobRepo = _locator.GetInstance<IScheduledJobRepository>();
            var job = jobRepo.Get(this.ScheduledJobId);
            var nextExecutionUTC = job.NextExecutionUTC;
            var autoPublishTestResults = true;

            // throw this in a try in case we can't access the big table for some reason, that shouldn't be a reason to not be able to create a test - this really shouldn't happen though.
            try
            {
                autoPublishTestResults = AdminConfigTestSettings.Current.AutoPublishWinner;
            }
            catch { }
            
            // Start / stop any tests that need to be.
            // If any tests are scheduled to start or stop prior to the next scheduled
            // exection date of this job, change the next execution date approprately. 
            foreach (var test in webRepo.GetTestList(new TestCriteria()))
            {
                switch (test.State)
                {
                    case TestState.Active:
                        var utcEndDate = test.EndDate;
                        if (DateTime.UtcNow > utcEndDate) // stop it now
                        {
                            webRepo.StopMarketingTest(test.Id);

                            //calculate significance results
                            var sigResults = Significance.CalculateIsSignificant(test);
                            test.IsSignificant = sigResults.IsSignificant;
                            test.ZScore = sigResults.ZScore;

                            if (autoPublishTestResults && sigResults.IsSignificant)
                            {
                                if (Guid.Empty != sigResults.WinningVariantId)
                                {
                                    var winningVariant = test.Variants.First(v => v.Id == sigResults.WinningVariantId);
                                    winningVariant.IsWinner = true;

                                    webRepo.SaveMarketingTest(test);

                                    var contextData = testingContextHelper.GenerateContextData(test);
                                    var winningLink = winningVariant.IsPublished ? contextData.PublishedVersionContentLink : contextData.DraftVersionContentLink;

                                    var storeModel = new TestResultStoreModel()
                                    {
                                        DraftContentLink = contextData.DraftVersionContentLink,
                                        PublishedContentLink = contextData.PublishedVersionContentLink,
                                        TestId = test.Id.ToString(),
                                        WinningContentLink = winningLink
                                    };

                                    webRepo.PublishWinningVariant(storeModel);
                                }
                            }
                            else
                            {
                                webRepo.SaveMarketingTest(test);
                            }

                            stopped++;
                        }
                        else if (nextExecutionUTC > utcEndDate)
                        {
                            // set a newer date to run the job again
                            nextExecutionUTC = utcEndDate;
                        }
                        break;
                    case TestState.Inactive:
                        var utcStartDate = test.StartDate;
                        if ( DateTime.UtcNow > utcStartDate) // start it now
                        {
                            webRepo.StartMarketingTest(test.Id);
                            started++;
                        }
                        else if (nextExecutionUTC > utcStartDate)
                        {
                            // set a newer date to run the job again
                            nextExecutionUTC = utcStartDate;
                        }
                        break;
                }
            }

            // update the next run time if we need to
            if( job.NextExecutionUTC != nextExecutionUTC )
            {
                if (job.IsEnabled)
                {
                    // NextExecution requires local time
                    job.NextExecution = nextExecutionUTC.ToLocalTime();
                    jobRepo.Save(job);
                }
            }

            // Calculate active, inactive and done for log message
            foreach ( var test in webRepo.GetTestList(new TestCriteria()) )
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
