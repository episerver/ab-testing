using System;

namespace EPiServer.Marketing.Testing.Core.Messaging.Messages
{
    /// <summary>
    /// Message used to increment the view count for a specific variant version of a test.
    /// </summary>
    public class UpdateViewsMessage
    {
        /// <summary>
        /// ID of the test.
        /// </summary>
        public Guid TestId { get; set; }

        /// <summary>
        /// Version of the variant to increment its view count.
        /// </summary>
        public int ItemVersion { get; set; }
    }
}
