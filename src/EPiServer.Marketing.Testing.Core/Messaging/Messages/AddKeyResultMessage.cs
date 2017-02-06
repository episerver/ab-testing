using System;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;

namespace EPiServer.Marketing.Testing.Core.Messaging.Messages
{
    /// <summary>
    /// Message used for saving any type of kpi result that implements IKeyResult.
    /// </summary>
    public class AddKeyResultMessage
    {
        /// <summary>
        /// Id of the test that the kpi result pertains to.
        /// </summary>
        public Guid TestId { get; set; }

        /// <summary>
        /// Version of the variant that the kpi result pertains to.
        /// </summary>
        public int ItemVersion { get; set; }

        /// <summary>
        /// The result data to save.
        /// </summary>
        public IKeyResult Result { get; set; }

        /// <summary>
        /// The type of result data to save.
        /// </summary>
        public KeyResultType Type { get; set; }

    }
}
