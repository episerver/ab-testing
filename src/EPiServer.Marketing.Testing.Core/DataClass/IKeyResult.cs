using System;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    /// <summary>
    /// This interface defines the minimal required fields for a KPI result to contain in order to be tied to a KPI that is part of a test.
    /// </summary>
    public interface IKeyResult
    {
        /// <summary>
        /// ID of a result recored.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// ID of KPI this result is associated with.
        /// </summary>
        Guid KpiId { get; set; }

        /// <summary>
        /// Date this result was created.
        /// </summary>
        DateTime CreatedDate { get; set; }

        /// <summary>
        /// Date this result was modified. 
        /// </summary>
        DateTime ModifiedDate { get; set; }
    }
}
