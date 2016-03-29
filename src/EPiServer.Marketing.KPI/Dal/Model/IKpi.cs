using System;

namespace EPiServer.Marketing.KPI.Dal.Model
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
        /// The condition to be met for the kpi to be met by a user.
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
        /// 
        /// </summary>
        /// <param name="theValues"></param>
        /// <returns></returns>
        bool Evaluate(object theValues);

    }
}
