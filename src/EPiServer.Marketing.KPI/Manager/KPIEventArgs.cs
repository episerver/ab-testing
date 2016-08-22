using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Marketing.KPI.Manager.DataClass;

namespace EPiServer.Marketing.KPI.Manager
{
    public class KpiEventArgs : EventArgs
    {
        public KpiEventArgs(Kpi kpi)
        {
            this.Kpi = kpi;
        }

        public Kpi Kpi { get; private set; }
    }
}
