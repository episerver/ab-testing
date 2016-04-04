using EPiServer.Core;
using System;

namespace EPiServer.Marketing.KPI.Manager.DataClass
{
    /// <summary>
    /// Interface for KPI objects.
    /// </summary>
    public interface IKpi
    {
        /// <summary>
        /// Id of Kpi.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Properties { get; set; }

        /// <summary>
        /// Date the kpi was created.
        /// </summary>
        DateTime CreatedDate { get; set; }

        /// <summary>
        /// The last time the kpi was modified.
        /// </summary>
        DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Determines if a conversion has happened.
        /// </summary>
        /// <param name="content"></param>
        Boolean Evaluate(IContent content);
    }
}
