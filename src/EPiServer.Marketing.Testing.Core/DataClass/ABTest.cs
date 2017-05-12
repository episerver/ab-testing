using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    /// <summary>
    /// Central test object that keeps track of everything related to a marketing test.
    /// </summary>
    public class ABTest : CoreEntityBase, IMarketingTest
    {
        /// <inheritdoc />
        public string Title { get; set; }

        /// <inheritdoc />
        public string Description { get; set; }

        /// <inheritdoc />
        public string Owner { get; set; }

        /// <inheritdoc />
        public Guid OriginalItemId { get; set; }

        /// <inheritdoc />
        public TestState State { get; set; }

        /// <inheritdoc />
        public DateTime StartDate { get; set; }

        /// <inheritdoc />
        public DateTime EndDate { get; set; }

        /// <inheritdoc />
        public int ParticipationPercentage { get; set; }

        /// <inheritdoc />
        public double ConfidenceLevel { get; set; }

        /// <inheritdoc />
        public double ZScore { get; set; }

        /// <inheritdoc />
        public bool IsSignificant { get; set; }

        /// <inheritdoc />
        public string LastModifiedBy { get; set; }

        /// <inheritdoc />
        public List<Variant> Variants { get; set; }

        /// <inheritdoc />
        public List<IKpi> KpiInstances { get; set; }

    }
}
