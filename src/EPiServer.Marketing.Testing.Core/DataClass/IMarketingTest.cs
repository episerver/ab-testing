using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    /// <summary>
    /// Defines the minimum data set required for a marketing test.
    /// </summary>
    public interface IMarketingTest
    {
        /// <summary>
        /// Test ID.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Test name.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Test Description.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Creator of the test.
        /// </summary>
        string Owner { get; set; }

        /// <summary>
        /// Item being tested against.  This is the current published version of content.
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
        DateTime EndDate { get; set; }

        /// <summary>
        /// Percentage of users that visit the site to include in the test.
        /// </summary>
        int ParticipationPercentage { get; set; }

        /// <summary>
        /// Percentage of accuracy required.
        /// Default: 95 %
        /// </summary>
        double ConfidenceLevel { get; set; }

        /// <summary>
        /// Calculated Z-Score to determine statistical significance.
        /// </summary>
        double ZScore { get; set; }

        /// <summary>
        /// Once the test is completed, this is filled in based on the statistical calculations around significance.
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
        /// List of variant items.  These replace the OriginalItem during the test.
        /// </summary>
        List<Variant> Variants { get; set; }

        /// <summary>
        /// List of KPIs.  These can be time on a page, form submission, etc.
        /// </summary>
        List<IKpi> KpiInstances { get; set; }
    }
}
