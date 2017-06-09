using System;

namespace EPiServer.Marketing.Testing.Core.Messaging.Messages
{
    /// <summary>
    /// Message used to increment the conversion count for a specific variant version of a test.
    /// </summary>
    public class UpdateConversionsMessage
    {
        /// <summary>
        /// ID of a test.
        /// </summary>
        public Guid TestId { get; set; }

        /// <summary>
        /// Version of the variant to increment its conversion count.
        /// </summary>
        public int ItemVersion { get; set; }

        public Guid KpiId { get; set; }

        /// <summary>
        /// Set this property to a unique value that is client specific to prevent aggressive clients from triggering multiple conversions.
        /// </summary>
        public string ClientIdentifier { get; set; }
    }
}
