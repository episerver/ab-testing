using System;

namespace EPiServer.Marketing.Testing.Core.Messaging.Messages
{
    /// <summary>
    /// Message used to increment the conversion count for a specific variant version of a test.
    /// </summary>
    public class UpdateConversionsMessage
    {
        /// <summary>
        /// Id of a test.
        /// </summary>
        public Guid TestId { get; set; }

        /// <summary>
        /// Version of the variant to increment its conversion count.
        /// </summary>
        public int ItemVersion { get; set; }

        public Guid KpiId { get; set; }
    }
}
