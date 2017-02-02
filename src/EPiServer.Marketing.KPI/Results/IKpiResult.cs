using System;

namespace EPiServer.Marketing.KPI.Results
{
    public interface IKpiResult
    {
        /// <summary>
        /// Id of the Kpi that the result pertains.
        /// </summary>
        Guid KpiId { get; set; }

        /// <summary>
        /// Indicates that all conditions for a conversion have been met.
        /// </summary>
        bool HasConverted { get; set; }
    }
}
