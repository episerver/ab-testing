using System;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;

namespace EPiServer.Marketing.Testing.Messaging
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMessagingManager
    {
        /// <summary>
        /// Emits the asynchronous message to update the view result for the specified Variant version.
        /// </summary>
        /// <param name="testId">Id of a test.</param>
        /// <param name="itemVersion">Version of the cms item that was viewed.</param>
        void EmitUpdateViews(Guid testId, int itemVersion);

        /// <summary>
        /// Emits the asynchronous message to update a conversion result for the specified Variant version.
        /// </summary>
        /// <param name="testId">Id of a test.</param>
        /// <param name="itemVersion">Version of the cms item that caused a conversion.</param>
        void EmitUpdateConversion(Guid testId, int itemVersion);

        /// <summary>
        /// Emits the asynchronous message to add a kpi result to the specified Variant version.
        /// </summary>
        /// <param name="testId">Id of a test.</param>
        /// <param name="itemVersion">Version of the cms item that caused a conversion.</param>
        /// <param name="keyResult">Result containing data pertinent to the kpi.</param>
        /// <param name="type">Type of the kpi result.</param>
        void EmitKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type);
        
        /// <summary>
        /// Returns the number of items in the queue
        /// </summary>
        int Count { get; }
    }
}
