using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EPiServer.Marketing.KPI.Common
{
    public interface IKpiResult
    {
        Guid Id { get; set; }
        
    }

    public class KpiResult : IKpiResult
    {
        public Guid Id { get; set; }

        public bool HasConverted { get; set; }

    }


}
