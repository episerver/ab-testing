using System;
using System.Collections.Generic;
using System.Security.Principal;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Security;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// Used with testmanager Kpi related events
    /// </summary>
    public class KpiEventArgs : TestEventArgs
    {
        public KpiEventArgs(IKpi kpi, IMarketingTest marketingTest) : base(marketingTest)
        {
            this.Kpi = kpi;
            CurrentUser = PrincipalInfo.Current.Principal.Identity;
        }

        public KpiEventArgs(IDictionary<Guid,bool> kpiConversionDictionary, IMarketingTest marketingTest) : base(marketingTest)
        {
            KpiConversionDictionary = kpiConversionDictionary;
        }

        
        public IKpi Kpi { get; private set; }
        public IDictionary<Guid,bool> KpiConversionDictionary { get; private set; }
    }
}

