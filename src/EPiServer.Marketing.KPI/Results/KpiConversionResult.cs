using System;

namespace EPiServer.Marketing.KPI.Results
{
    /// <summary>
    /// Boolean conversion result for kpi's.
    /// </summary>
    public class KpiConversionResult : IKpiResult
    {
        /// <inheritdoc />
        public Guid KpiId { get; set; }

        /// <inheritdoc />
        public bool HasConverted { get; set; }

    }
}
