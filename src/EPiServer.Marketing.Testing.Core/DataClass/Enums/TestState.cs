namespace EPiServer.Marketing.Testing.Core.DataClass.Enums
{
    /// <summary>
    /// Defines the possible states that a test can be in at any given time.
    /// </summary>
    public enum TestState
    {
        /// <summary>
        /// Test has been created and scheduled to be run but has not reached its start date yet.
        /// </summary>
        Inactive = 0,

        /// <summary>
        /// Test has started and is currently running.
        /// </summary>
        Active = 1,

        /// <summary>
        /// Test has completed by reaching its end date.
        /// </summary>
        Done = 2,

        /// <summary>
        /// A winning variant has been picked and published.
        /// </summary>
        Archived = 3
    }
}
