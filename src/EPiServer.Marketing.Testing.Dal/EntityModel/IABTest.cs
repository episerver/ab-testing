using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;

namespace EPiServer.Marketing.Testing.Dal.EntityModel
{
    public interface IABTest
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
        DalTestState State { get; set; }

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
        IList<DalVariant> Variants { get; set; }

        /// <summary>
        /// List of key performance indicators.  These can be time on a page, form submission, etc.
        /// </summary>
        IList<DalKeyPerformanceIndicator> KeyPerformanceIndicators { get; set; }
    }
}
