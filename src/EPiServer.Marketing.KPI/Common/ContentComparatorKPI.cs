using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager.DataClass;
using System;

namespace EPiServer.Marketing.KPI.Common
{
    /// Common KPI class that can be used to compare IContent Guid values 
    /// 
    public class ContentComparatorKPI : Kpi
    {
        private Guid _ContentGuid;

        public ContentComparatorKPI(Guid contentGuid)
        {
            Properties = contentGuid.ToString();
        }

        public override string Properties {
            set { _ContentGuid = Guid.Parse(value);  }
            get { return _ContentGuid.ToString();  }
        }

        public override Boolean Evaluate(IContent content)
        {
            return _ContentGuid.Equals(content.ContentGuid);
        }
    }
}
