using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Data.Enums;

namespace EPiServer.Marketing.Testing.Data
{
    public interface IMarketingTest
    {
        Guid Id { get; set; }

        /// <summary>
        /// Test name.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Test name.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Creator of the test.
        /// </summary>
        string Owner { get; set; }

        /// <summary>
        /// Item used to test against.
        /// </summary>
        Guid OriginalItemId { get; set; }

        /// <summary>
        /// Current state of the test.
        /// </summary>
        TestState State { get; set; }

        /// <summary>
        /// Date and time the test is scheduled to start.
        /// </summary>
        DateTime StartDate { get; set; }

        /// <summary>
        /// Date and time the test is scheduled to end.
        /// </summary>
        DateTime? EndDate { get; set; }

        /// <summary>
        /// Percentage of users to include in the test.
        /// </summary>
        int ParticipationPercentage { get; set; }

        /// <summary>
        /// Percentage of accuracy required.
        /// Default: 95 %
        /// </summary>
        double ConfidenceLevel { get; set; }

        /// <summary>
        /// Calculated z-score to determine statistical significance.
        /// </summary>
        double ZScore { get; set; }

        /// <summary>
        /// 
        /// </summary>
        bool IsSignificant { get; set; }

        /// <summary>
        /// The person that last changed the test.
        /// </summary>
        string LastModifiedBy { get; set; }

        /// <summary>
        /// Date and time the test was created.
        /// </summary>
        DateTime CreatedDate { get; set; }

        /// <summary>
        /// Last time the test was modified.
        /// </summary>
        DateTime ModifiedDate { get; set; }

        /// <summary>
        /// List of possible variant items.  These replace the OriginalItem during the test.
        /// </summary>
        List<Variant> Variants { get; set; }

        /// <summary>
        /// List of key performance indicators.  These can be time on a page, form submission, etc.
        /// </summary>
        List<IKpi> KpiInstances { get; set; }
    }
}
