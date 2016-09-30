using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Marketing.KPI.Manager.DataClass;

namespace EPiServer.Marketing.Testing.Web.Models
{
    public class KpiViewModel
    {
        public IKpi kpi { get; set; }
        public Type kpiType { get; set; }
    }
}
