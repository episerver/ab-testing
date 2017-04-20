using System;
using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    /// <summary>
    /// Represents the data stored in session cookies for users that visit an item under test.
    /// </summary>
    public class TestDataCookie
    {
        private IDictionary<Guid, bool> kpis = new Dictionary<Guid, bool>();

        /// <summary>
        /// Test Id.
        /// </summary>
        public Guid TestId { get; set; }

        /// <summary>
        /// Id of item under test.
        /// </summary>
        public Guid TestContentId { get; set; }

        /// <summary>
        /// Id of item variant being tested.
        /// </summary>
        public Guid TestVariantId { get; set; }

        /// <summary>
        /// States whether the current user sees the original item or the variant being tested.
        /// </summary>
        public bool ShowVariant { get; set; }

        /// <summary>
        /// Has the current user viewed the item.
        /// </summary>
        public bool Viewed { get; set; }

        /// <summary>
        /// Has the current user converted based on the kpi(s) associated with the test.
        /// </summary>
        public bool Converted { get; set; }

        /// <summary>
        /// States whether multiple conversions are possible.
        /// </summary>
        public bool AlwaysEval { get; set; }

        /// <summary>
        /// Keeps track of each kpi and its conversion state(i.e. converted or not).
        /// </summary>
        public IDictionary<Guid, bool> KpiConversionDictionary { get { return kpis; } }
    }
}