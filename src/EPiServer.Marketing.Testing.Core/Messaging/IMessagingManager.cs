using System;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;

namespace EPiServer.Marketing.Testing.Messaging
{
    /// <summary>
    /// Emits asynchronous messages for views, conversions, and KPI results.
    /// </summary>
    public interface IMessagingManager
    {
        /// <summary>
        /// Emits the asynchronous message to update the view result for the specified Variant version.
        /// </summary>
        /// <param name="testId">ID of a test.</param>
        /// <param name="itemVersion">Version of the cms item that was viewed.</param>
        /// <param name="clientId">Optional client ID that will be used to throttle agressive clients and prevent multiple views.</param>
        void EmitUpdateViews(Guid testId, int itemVersion, string clientId = null);

        /// <summary>
        /// Emits the asynchronous message to update a conversion result for the specified Variant version.
        /// </summary>
        /// <param name="testId">ID of a test.</param>
        /// <param name="itemVersion">Version of the CMS item that caused a conversion.</param>
        /// <param name="kpiId">ID of the KPI that caused a conversion.</param>
        /// <param name="clientId">Optional client ID that will be used to throttle agressive clients and prevent multiple conversions.</param>
        void EmitUpdateConversion(Guid testId, int itemVersion, Guid kpiId = default(Guid), string clientId=null);

        /// <summary>
        /// Emits the asynchronous message to add a KPI result to the specified Variant version.
        /// </summary>
        /// <param name="testId">ID of a test.</param>
        /// <param name="itemVersion">Version of the CMS item that caused a conversion.</param>
        /// <param name="keyResult">Result containing data pertinent to the KPI.</param>
        /// <param name="type">Type of the KPI result.</param>
        void EmitKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type);
        
        /// <summary>
        /// Returns the number of items in the queue.
        /// </summary>
        int Count { get; }
    }
}
