using System;

namespace EPiServer.Marketing.KPI.Dal.Model
{
    /// <summary>
    /// Interface for KPI objects.
    /// </summary>
    public interface IDalKpi
    {
        /// <summary>
        /// Id of DalKpi.
        /// </summary>
        Guid Id { get; set; }

        string ClassName { get; set; }

        /// <summary>
        /// The condition to be met for the DalKpi to be met by a user.
        /// </summary>
        string Properties { get; set; }

        /// <summary>
        /// Date the DalKpi was created.
        /// </summary>
        DateTime CreatedDate { get; set; }

        /// <summary>
        /// The last time the DalKpi was modified.
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
