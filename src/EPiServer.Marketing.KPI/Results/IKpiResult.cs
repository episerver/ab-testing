using System;

namespace EPiServer.Marketing.KPI.Results
{
    public interface IKpiResult
    {
        Guid KpiId { get; set; }

        bool HasConverted { get; set; }
    }
}
