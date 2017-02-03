using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Security;

namespace EPiServer.Marketing.Testing
{
    /// <summary>
    /// Used with testmanager Kpi related events
    /// </summary>
    public class KpiEventArgs : TestEventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public IKpi Kpi { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<Guid, bool> KpiConversionDictionary { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpi"></param>
        /// <param name="marketingTest"></param>
        public KpiEventArgs(IKpi kpi, IMarketingTest marketingTest) : base(marketingTest)
        {
            this.Kpi = kpi;
            CurrentUser = PrincipalInfo.Current.Principal.Identity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kpiConversionDictionary"></param>
        /// <param name="marketingTest"></param>
        public KpiEventArgs(IDictionary<Guid,bool> kpiConversionDictionary, IMarketingTest marketingTest) : base(marketingTest)
        {
            KpiConversionDictionary = kpiConversionDictionary;
        }
    }
}

