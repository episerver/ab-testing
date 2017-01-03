using System;

namespace EPiServer.Marketing.KPI.Results
{
    public interface IKpiResult
    {
        Guid KpiId { get; set; }

        /// <summary>
        /// Indicates conditions for a conversion have been met.
        /// </summary>
        bool HasConverted { get; set; }
    }
}
