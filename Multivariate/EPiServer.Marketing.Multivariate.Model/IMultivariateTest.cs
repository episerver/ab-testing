﻿using System;
using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Model.Enums;

namespace EPiServer.Marketing.Multivariate.Model
{
    public interface IMultivariateTest
    {
        Guid Id { get; set; }

        /// <summary>
        /// Test name.
        /// </summary>
        string Title { get; set; }

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
        TestState TestState { get; set; }

        /// <summary>
        /// Date and time the test is scheduled to start.
        /// </summary>
        DateTime StartDate { get; set; }

        /// <summary>
        /// Date and time the test is scheduled to end.
        /// </summary>
        DateTime? EndDate { get; set; }

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
        /// List of possible conversions.
        /// </summary>
        IList<Conversion> Conversions { get; set; }

        /// <summary>
        /// List of possible variant items.  These replace the OriginalItem during the test.
        /// </summary>
        IList<Variant> Variants { get; set; }

        /// <summary>
        /// List of test results. There will be a MultivariateTestResult for the OriginalItem and each Variant item.
        /// </summary>
        IList<MultivariateTestResult> MultivariateTestResults { get; set; }

        /// <summary>
        /// List of key performance indicators.  These can be time on a page, form submission, etc.
        /// </summary>
        IList<KeyPerformanceIndicator> KeyPerformanceIndicators { get; set; }
    }
}
