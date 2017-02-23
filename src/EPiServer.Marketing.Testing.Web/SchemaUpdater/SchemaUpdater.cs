using System;
using System.Configuration;
using System.Data.SqlClient;
using EPiServer.Data;
using EPiServer.Data.SchemaUpdates;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Dal.Migrations;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.SchemaUpdater
{
    [ServiceConfiguration(typeof(IDatabaseSchemaUpdater))]
    public class DatabaseVersionValidator : IDatabaseSchemaUpdater
    {
        private const string UpdateDatabaseResource = "EPiServer.Marketing.Testing.Web.SchemaUpdater.Testing.zip";
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
            long version = 0;
            ITestManager testManager;
            ServiceLocator.Current.TryGetExistingInstance<ITestManager>(out testManager);

            version = testManager.GetDatabaseVersion(dbConnection, DatabaseVersion.Schema, DatabaseVersion.ContextKey);

            if (version < DatabaseVersion.RequiredDbVersion)
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

            ITestManager testManager;
            ServiceLocator.Current.TryGetExistingInstance<ITestManager>(out testManager);

            var dbConnection = new SqlConnection(_databaseHandler.ConnectionSettings.ConnectionString);
            var version = testManager.GetDatabaseVersion(dbConnection, DatabaseVersion.Schema, DatabaseVersion.ContextKey, true);

            if (DatabaseVersion.RequiredDbVersion != version)
            {
                //something went wrong!
            }
        }
    }
}
