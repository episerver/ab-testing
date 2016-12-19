using System;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;

namespace EPiServer.Marketing.Testing.Messaging
{
    public interface IMessagingManager
    {
        /// <summary>
        /// Emits the asynchronous message to update the view result for the specified VariantId
        /// </summary>
        /// <param name="TestId">the test id to work with</param>
        /// <param name="VariantId">the Guid of the cms item that was viewed</param>
        void EmitUpdateViews(Guid TestId, int itemVersion);

        /// <summary>
        /// Emits the asynchronous message to update a conversion result for the specified VariantId
        /// </summary>
        /// <param name="TestId"></param>
        /// <param name="VariantId">the Guid of the cms item that caused a converion</param>
        void EmitUpdateConversion(Guid TestId, int itemVersion);

        void EmitKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type);
        
        /// <summary>
        /// Returns the number of items in the queue
        /// </summary>
        int Count { get; }
    }
}
