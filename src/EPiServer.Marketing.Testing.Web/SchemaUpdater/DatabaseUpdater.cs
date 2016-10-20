using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Migrations.History;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Data;
using EPiServer.Data.SchemaUpdates;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.SchemaUpdater
{
    [ServiceConfiguration(typeof(IDatabaseSchemaUpdater))]
    public class DatabaseVersionValidator : IDatabaseSchemaUpdater
    {
        internal const long RequiredDatabaseVersion = 201609291731241;
        internal const string UpdateDatabaseResource = "EPiServer.Marketing.Testing.Web.SchemaUpdater.Testing.zip";

        private readonly IDatabaseHandler _databaseHandler;
        private readonly ScriptExecutor _scriptExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseVersionValidator"/> class.
        /// </summary>
        public DatabaseVersionValidator(IDatabaseHandler databaseHandler, ScriptExecutor scriptExecutor)
        {
            _databaseHandler = databaseHandler;
            _scriptExecutor = scriptExecutor;
        }

        /// <inheritdoc/>
        public DatabaseSchemaStatus GetStatus(ConnectionStringsSection connectionStrings)
        {
            var dbConnection = new SqlConnection(_databaseHandler.ConnectionSettings.ConnectionString);
            var hc = new HistoryContext(dbConnection, "dbo");
            var history = hc.History;
            var lastMigration = history.Where(r => r.ContextKey == "Testing.Migrations.Configuration").OrderByDescending(row => row.MigrationId).First();

            var version = lastMigration.MigrationId.Split('_')[0];

            if (Convert.ToInt64(version) < RequiredDatabaseVersion)
            {
                // need to upgrade, versions can only be int, so we force it with fake versions based off our real veresions which are longs from EF
                return new DatabaseSchemaStatus
                {
                    ConnectionStringSettings = _databaseHandler.ConnectionSettings,
                    ApplicationRequiredVersion = new Version(2, 0),
                    DatabaseVersion = new Version(1, 0)
                };
            }

            // don't need to upgrade
            return new DatabaseSchemaStatus
            {
                ConnectionStringSettings = _databaseHandler.ConnectionSettings,
                ApplicationRequiredVersion = new Version(1, 0),
                DatabaseVersion = new Version(1, 0)
            };
        }

        /// <inheritdoc/>
        public void Update(ConnectionStringSettings connectionStringSettings)
        {
            _scriptExecutor.OrderScriptsByVersion = true;
            _scriptExecutor.ExecuteEmbeddedZippedScripts(connectionStringSettings.ConnectionString, typeof(DatabaseVersionValidator).Assembly, UpdateDatabaseResource);
        }
    }
}
