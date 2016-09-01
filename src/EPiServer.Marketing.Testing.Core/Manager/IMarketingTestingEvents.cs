using System;
using EPiServer.Marketing.Testing.Core.Manager;


namespace EPiServer.Marketing.Testing
{
    public interface IMarketingTestingEvents
    {
        /// <summary>
        /// Occurs after a test has been created or updated and all related state and cache changes
        /// have been made.
        /// </summary>
        event EventHandler<TestEventArgs> TestSaved;

        /// <summary>
        /// Occurs after a test has been deleted and all related state and cache changes
        /// have been made
        /// </summary>
        event EventHandler<TestEventArgs> TestDeleted;

        /// <summary>
        /// Occurs after a test has transitioned to the started state and cache changes made
        /// </summary>
        event EventHandler<TestEventArgs> TestStarted;

        /// <summary>
        /// Occurs after a test has transitioned to the stopped state and cache changes made
        /// </summary>
        event EventHandler<TestEventArgs> TestStopped;

        /// <summary>
        /// Occurs after a test has transitioned to the archived state. Content has been published
        /// and cache changes have been made.
        /// </summary>
        event EventHandler<TestEventArgs> TestArchived;

        /// <summary>
        /// Occurs when content has the published content has been switched out with the
        /// test variant
        /// </summary>
        event EventHandler<ContentEventArgs> ContentSwitched;

        /// <summary>
        /// Occurs when a user is included in an active test.
        /// </summary>
        event EventHandler<TestEventArgs> UserIncludedInTest;

        /// <summary>
        /// Occurs when a Kpi is successfully converted.
        /// </summary>
        event EventHandler<KpiEventArgs> KpiConverted;

    }
}
