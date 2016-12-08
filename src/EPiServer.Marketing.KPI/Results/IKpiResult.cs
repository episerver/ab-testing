using System;

namespace EPiServer.Marketing.KPI.Results
{
    public interface IKpiResult
    {
        Guid KpiId { get; set; }

        /// <summary>
        /// Indicates test conditions have been met
        /// </summary>
        bool HasConverted { get; set; }
    }
}
