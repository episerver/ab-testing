using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Web.Statistics;
using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.Security;
using EPiServer.Marketing.Testing.Core.Manager;

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
        private AdminConfigTestSettings _config;

        [ExcludeFromCodeCoverage]
        public TestSchedulingJob()
        {
            _locator = ServiceLocator.Current;
            _config = AdminConfigTestSettings.Current;
        }

        internal TestSchedulingJob(IServiceLocator locator)
        {
            _locator = locator;
            _config = _locator.GetInstance<AdminConfigTestSettings>();
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

            var autoPublishTestResults = _config.AutoPublishWinner;
            
            // Start / stop any tests that need to be.
            // If any tests are scheduled to start or stop prior to the next scheduled
            // exection date of this job, change the next execution date approprately. 
            foreach (var test in webRepo.GetTestList(new TestCriteria()))
            {
                switch (test.State)
                {
                    case TestState.Active:
                        var utcEndDate = test.EndDate.ToUniversalTime();
                        if (DateTime.UtcNow > utcEndDate) // stop it now
                        {
                            webRepo.StopMarketingTest(test.Id);

                            UpdateTest(test, webRepo, testingContextHelper, autoPublishTestResults);

                            stopped++;
                        }
                        else if (nextExecutionUTC > utcEndDate)
                        {
                            // set a newer date to run the job again
                            nextExecutionUTC = utcEndDate;
                        }
                        else
                        {
                            // MAR-1180 - in a load balanced env the job is not putting the test in the active cache list. 
                            var activeTest = webRepo.GetActiveCachedTests().FirstOrDefault(t => t.Id == test.Id);
                            if (activeTest == null)
                            {
                                // Add to the active list since its not there but it is active.
                                webRepo.UpdateCache(test, CacheOperator.Add);
                                started++;
                            }
                        }
                        break;
                    case TestState.Inactive:
                        var utcStartDate = test.StartDate.ToUniversalTime();
                        if (DateTime.UtcNow > utcStartDate) // start it now
                        {
                            webRepo.StartMarketingTest(test.Id);
                            started++;
                        }
                        else if (nextExecutionUTC > utcStartDate)
                        {
                            // set a newer date to run the job again
                            nextExecutionUTC = utcStartDate;
                        }
                        else
                        {
                            // MAR-1180 - in a load balanced env the job is not adding the test to the cache
                            var inActiveTest = webRepo.GetActiveCachedTests().FirstOrDefault(t => t.Id == test.Id);
                            if (inActiveTest != null)
                            {
                                // Add to the active list since its not there but it is active.
                                webRepo.UpdateCache(test, CacheOperator.Remove);
                                stopped++;
                            }
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

        /// <summary>
        /// Calculate test results to see if they are significant or not and updat the test.  Also, autopublish the winning variant if the results are significant and autopublish is enabled.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="webRepo"></param>
        /// <param name="testingContextHelper"></param>
        /// <param name="autoPublishTestResults"></param>
        private void UpdateTest(IMarketingTest test, IMarketingTestingWebRepository webRepo, ITestingContextHelper testingContextHelper, bool autoPublishTestResults)
        {
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

                    // We need to impersonate the user that created the test because the job may not have sufficient priviledges.  If there is no context(i.e. someone didn't force run the job) then 
                    // the test creator will be used and the log will show this user name.
                    if (HttpContext.Current == null)
                    {
                        PrincipalInfo.CurrentPrincipal = PrincipalInfo.CreatePrincipal(test.Owner);
                    }

                    webRepo.PublishWinningVariant(storeModel);
                }
            }
            else
            {
                webRepo.SaveMarketingTest(test);
            }
        }
    }
}
