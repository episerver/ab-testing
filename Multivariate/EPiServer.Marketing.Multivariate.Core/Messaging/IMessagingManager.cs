using System;

namespace EPiServer.Marketing.Multivariate.Messaging
{
    public interface IMessagingManager
    {
        /// <summary>
        /// Emits the asynchronous message to update the view result for the specified VariantId
        /// </summary>
        /// <param name="TestId">the test id to work with</param>
        /// <param name="VariantId">the Guid of the cms item that was viewed</param>
        void EmitUpdateViews(Guid TestId, Guid VariantId);

        /// <summary>
        /// Emits the asynchronous message to update a conversion result for the specified VariantId
        /// </summary>
        /// <param name="TestId"></param>
        /// <param name="VariantId">the Guid of the cms item that caused a converion</param>
        void EmitUpdateConversion(Guid TestId, Guid VariantId);
    }
}
