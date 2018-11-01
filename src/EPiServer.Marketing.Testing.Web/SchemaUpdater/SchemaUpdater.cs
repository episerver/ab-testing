using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Data;
using EPiServer.Data.SchemaUpdates;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Dal.Migrations;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Testing.Web.SchemaUpdater
{
    /// <summary>
    /// Used by automatic database upgrades.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [ServiceConfiguration(typeof(ISchemaUpdater))]
    public class SchemaUpdater : ISchemaUpdater
    {
        private const string UpgradeScriptResource = "EPiServer.Marketing.Testing.Web.SchemaUpdater.Testing.zip";
        private readonly IDatabaseHandler _databaseHandler;
        private readonly ScriptExecutor _scriptExecutor;
        private ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaUpdater"/> class.
        /// </summary>
        public SchemaUpdater(IDatabaseHandler databaseHandler, ScriptExecutor scriptExecutor)
        {
            _databaseHandler = databaseHandler;
            _scriptExecutor = scriptExecutor;
            _logger = LogManager.GetLogger();
        }

        /// <inheritdoc/>
        public SchemaStatus GetStatus(IEnumerable<ConnectionStringOptions> connectionStringOptions)
        {
            try
            {
                var dbConnection = new SqlConnection(_databaseHandler.ConnectionSettings.ConnectionString);
                long version = 0;
                // MAR-1205 : we need to get a new instance here, because most likely the dataaccess layer won't be available, by the time
                // a new instance of test manager is needed, dataaccesss will be initialized and all will be well
                ITestManager testManager = new TestManager();

                version = testManager.GetDatabaseVersion(dbConnection, DatabaseVersion.Schema, DatabaseVersion.ContextKey);

                if (version < DatabaseVersion.RequiredDbVersion)
                {
                    // need to upgrade, versions can only be int, so we force it with fake versions based off our real veresions which are longs from EF
                    return new SchemaStatus()
                    {
                        ConnectionStringOption = _databaseHandler.ConnectionSettings.ConnectionString,
                        ApplicationRequiredVersion = new Version(2, 0),
                        DatabaseVersion = new Version(1, 0)
                    };
                }

                // don't need to upgrade
                return new SchemaStatus()
                {
                    ConnectionStringOption = _databaseHandler.ConnectionSettings.ConnectionString,
                    ApplicationRequiredVersion = new Version(1, 0),
                    DatabaseVersion = new Version(1, 0)
                };
            }
            catch (Exception ex)
            {
                _logger.Error("ABTesting: Unable to retrieve version of scehema.", ex);
                return null;
            }

        }

        /// <inheritdoc/>
        public void Update(ConnectionStringOptions connectionStringOptions)
        {
            try
            {
                _scriptExecutor.OrderScriptsByVersion = true;
                _scriptExecutor.ExecuteEmbeddedZippedScripts(connectionStringOptions.ConnectionString, typeof(SchemaUpdater).Assembly, UpgradeScriptResource);
            }
            catch (Exception ex)
            {
                _logger.Error("ABTesting: Error upgrading schema.", ex);
            }
        }
    }
}

