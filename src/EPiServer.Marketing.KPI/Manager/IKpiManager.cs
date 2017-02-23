using System;
using System.Collections.Generic;
using System.Data.Common;
using EPiServer.Marketing.KPI.Manager.DataClass;

namespace EPiServer.Marketing.KPI.Manager
{
    /// <summary>
    /// This manages the CRUD operations for kpi types.  It also handles finding all available kpi types as well as retrieving some database info around upgrades.
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
        /// Gets the list of all KPI objects.
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

        /// <summary>
        /// If the database needs to be configured, then we return so that it can be set up.  If it has already been configured, we get the version of the current kpi schema and upgrade it if it is an older version.
        /// </summary>
        /// <param name="dbConnection">Connection properties for the desired database to connect to.</param>
        /// <param name="schema">Schema that should be applied to the database (upgrade or downgrade) if the database is outdated.</param>
        /// <param name="contextKey">The string used to identify the schema we are requesting the version of.</param>
        /// <param name="setupDataAccess">If this is run before the database is setup, we need to initialize the database access layer.  By default, this is false.</param>
        /// <returns>Database version of the kpi schema.</returns>
        long GetDatabaseVersion(DbConnection dbConnection, string schema, string contextKey, bool setupDataAccess = false );

        /// <summary>
        /// Save commerce settings to the database.
        /// </summary>
        /// <param name="commerceSettings">Commerce settings to be saved.</param>
        void SaveCommerceSettings(CommerceData commerceSettings);

        /// <summary>
        /// Retrieves commerce setttings to be used with kpi's.
        /// </summary>
        /// <returns>Settings that have to do with commerce.  If no settings are found, then a default set is returned.</returns>
        CommerceData GetCommerceSettings();

    }
}
