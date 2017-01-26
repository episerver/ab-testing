using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.KPI.Manager.DataClass
{
    public interface IClientKpi
    {
        /// <summary>
        /// Scripts used to evaluate kpi conversion conditions
        /// </summary>
        string ClientEvaluationScript { get; }
    }
}
