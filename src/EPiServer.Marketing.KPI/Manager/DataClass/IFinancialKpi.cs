using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.KPI.Manager.DataClass
{
    /// <summary>
    /// Properties and methods for currency related KPIs.
    /// </summary>
    public interface IFinancialKpi
    {
        /// <summary>
        /// The preferred currency formatting to apply to all financial results.
        /// Assists in normalizing between different currencies prior to storage.
        /// </summary>
        CommerceData PreferredFinancialFormat { get; set; }
    }
}