using System;
using EPiServer.Marketing.KPI.Manager.DataClass;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// Used with testmanager Kpi related events
    /// </summary>
    public class KpiEventArgs : EventArgs
    {
        public KpiEventArgs(IKpi kpi)
        {
            this.Kpi = kpi;
        }

        public IKpi Kpi { get; private set; }
    }
}

