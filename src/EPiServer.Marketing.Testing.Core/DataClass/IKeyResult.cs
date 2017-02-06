using System;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    /// <summary>
    /// 
    /// </summary>
    public interface IKeyResult
    {
        /// <summary>
        /// Id of a result recored.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Id of kpi this result is associated with.
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
