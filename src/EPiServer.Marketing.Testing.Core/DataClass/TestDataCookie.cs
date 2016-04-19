using System;
using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    public class TestDataCookie
    {
        private IDictionary<Guid, bool> kpis = new Dictionary<Guid, bool>();

        public Guid TestId { get; set; }
        public Guid TestContentId { get; set; }
        public Guid TestVariantId { get; set; }
        public bool ShowVariant { get; set; }
        public bool Viewed { get; set; }
        public bool Converted { get; set; }
        public IDictionary<Guid, bool> KpiConversionDictionary { get { return kpis; } }
    }
}