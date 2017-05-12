using System;

namespace EPiServer.Marketing.KPI.Results
{
    /// <summary>
    /// Minimum information required to save a KPI conversion.
    /// </summary>
    public interface IKpiResult
    {
        /// <summary>
        /// ID of the KPI that the result pertains.
        /// </summary>
        Guid KpiId { get; set; }

        /// <summary>
        /// Indicates that all conditions for a conversion have been met.
        /// </summary>
        bool HasConverted { get; set; }
    }
}
