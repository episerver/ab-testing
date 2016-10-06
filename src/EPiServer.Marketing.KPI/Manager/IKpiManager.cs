using System;
using System.Collections.Generic;
using EPiServer.Marketing.KPI.Manager.DataClass;

namespace EPiServer.Marketing.KPI.Manager
{
    /// <summary>
    /// 
    /// </summary>
    public interface IKpiManager
    {
        /// <summary>
        /// Returns a KPI object based on its Id.
        /// </summary>
        /// <param name="kpiId">Id of the KPI to retrieve.</param>
        /// <returns>KPI object.</returns>
        IKpi Get(Guid kpiId);

        /// <summary>
        /// Gets the whole list of KPI objects.
        /// </summary>
        /// <returns>List of KPI objects.</returns>
        List<IKpi> GetKpiList();

        /// <summary>
        /// Adds or updates a KPI object.
        /// </summary>
        /// <param name="kpi">Id of the KPI to add/update.</param>
        /// <returns>The Id of the KPI object that was added/updated.</returns>
        Guid Save(IKpi kpi);

        /// <summary>
        /// Deletes KPI object from the db.
        /// </summary>
        /// <param name="kpiId">Id of the KPI to delete.</param>
        void Delete(Guid kpiId);

        /// <summary>
        /// Scans all assemblies and gets all instances of type IKPI
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetKpiTypes();

    }
}
