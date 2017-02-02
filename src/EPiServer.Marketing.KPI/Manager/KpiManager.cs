using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EPiServer.Marketing.KPI.Dal.Model;
using EPiServer.Marketing.KPI.DataAccess;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using StructureMap.TypeRules;
using EPiServer.Data.Dynamic;

namespace EPiServer.Marketing.KPI.Manager
{
    /// <summary>
    /// This manages the CRUD operations for kpi types.  It also handles finding all available kpi types as well as retrieving some database info around upgrades.
    /// </summary>
    [ServiceConfiguration(ServiceType = typeof(IKpiManager), Lifecycle = ServiceInstanceScope.Singleton)]
    public class KpiManager : IKpiManager
    {
        private IKpiDataAccess _dataAccess;
        private IServiceLocator _serviceLocator;
        public bool DatabaseNeedsConfiguring;

        /// <summary>
        /// Figures out if the database needs to be configured before setting up the data access layer.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public KpiManager()
        {
            _serviceLocator = ServiceLocator.Current;

            try
            {
                _dataAccess = new KpiDataAccess();
            }
            catch (DatabaseDoesNotExistException)
            {
                DatabaseNeedsConfiguring = true;
            }
        }

        /// <summary>
        /// Constructor used for unit testing with a mocked service locator.
        /// </summary>
        /// <param name="serviceLocator"></param>
        internal KpiManager(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _dataAccess = _serviceLocator.GetInstance<IKpiDataAccess>();
        }

        /// <summary>
        /// Retrieves a kpi from the database based on the id provided.
        /// </summary>
        /// <param name="kpiId">Id of a kpi.</param>
        /// <returns></returns>
        public IKpi Get(Guid kpiId)
        {
            return ConvertToManagerKpi(_dataAccess.Get(kpiId));
        }

        /// <summary>
        /// Given an A/B test, this retrieves all the list of kpi's associated with said test.
        /// </summary>
        /// <returns></returns>
        public List<IKpi> GetKpiList()
        {
            return _dataAccess.GetKpiList().Select(dalTest => ConvertToManagerKpi(dalTest)).ToList();
        }

        /// <summary>
        /// Saves a kpi to the database.
        /// </summary>
        /// <param name="kpi">The kpi to save.</param>
        /// <returns></returns>
        public Guid Save(IKpi kpi)
        {
            return _dataAccess.Save(ConvertToDalTest(kpi));
        }

        /// <summary>
        /// Deletes a kpi from the database.
        /// </summary>
        /// <param name="kpiId">The kpi to delete.</param>
        public void Delete(Guid kpiId)
        {
            _dataAccess.Delete(kpiId);
        }

        /// <summary>
        /// Retrieves all kpi types from assemblies in the current domain.  These types are displayed to the user when creating a test as a choice of what the test will measure
        /// </summary>
        /// <returns>All kpi types that implement IKpi in the known assemblies.</returns>
        public IEnumerable<Type> GetKpiTypes()
        {
            var type = typeof(IKpi);
            // exclude interfaces, abstract instances, and the convience base class Kpi
            var types =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p) && !p.IsInterfaceOrAbstract() && p != typeof(Kpi));
            return (types);
        }

        /// <summary>
        /// If the database needs to be configured, then we return so that it can be set up.  If it has already been configured, we get the version of the current kpi schema and upgrade it if it is an older version.
        /// </summary>
        /// <param name="dbConnection">Connection properties for the desired database to connect to.</param>
        /// <param name="schema">Schema that should be applied to the database (upgrade or downgrade) if the database is outdated.</param>
        /// <param name="contextKey">The string used to identify the schema we are requesting the version of.</param>
        /// <param name="setupDataAccess">If this is run before the database is setup, we need to initialize the database access layer.  By default, this is false.</param>
        /// <returns>Database version of the kpi schema.</returns>
        public long GetDatabaseVersion(DbConnection dbConnection, string schema, string contextKey, bool setupDataAccess = false)
        {
            if (DatabaseNeedsConfiguring)
            {
                DatabaseNeedsConfiguring = false;
                return 0;
            }

            if (setupDataAccess)
            {
                _dataAccess = new KpiDataAccess();
            }

            return _dataAccess.GetDatabaseVersion(dbConnection, schema, contextKey);
        }

        /// <summary>
        /// Save commerce settings to the database.
        /// </summary>
        /// <param name="commerceSettings">Commerce settings to be saved.</param>
        public void SaveCommerceSettings(CommerceData commerceSettings)
        {            
            var store = GetDataStore(typeof(CommerceData));
            store.Save(commerceSettings);
        }

        /// <summary>
        /// Retrieves commerce setttings to be used with kpi's.
        /// </summary>
        /// <returns>Settings that have to do with commerce.  If no settings are found, then a default set is returned.</returns>
        public CommerceData GetCommerceSettings()
        {
            var store = GetDataStore(typeof(CommerceData));
            var settings = store.LoadAll<CommerceData>().OrderByDescending(x => x.Id.StoreId).FirstOrDefault() ?? new CommerceData { CommerceCulture = "DEFAULT" };
            return settings;
        }

        private DynamicDataStore GetDataStore(Type t)
        {
            return DynamicDataStoreFactory.Instance.GetStore(t) ?? DynamicDataStoreFactory.Instance.CreateStore(t);
        }

        /// <summary>
        /// Serialize the kpi to a Json string and save it in the properties field.
        /// </summary>
        /// <param name="kpi">Kpi to save to the db (i.e. contentcomparatorkpi, timeonpagekpi, etc.</param>
        /// <returns>EF Kpi object to save in the db.</returns>
        private IDalKpi ConvertToDalTest(IKpi kpi)
        {
            if (Guid.Empty == kpi.Id)
            {   // if the kpi.id is null, its because we are creating a new one.
                kpi.Id = Guid.NewGuid();
                kpi.CreatedDate = DateTime.UtcNow;
                kpi.ModifiedDate = DateTime.UtcNow;
            }

            var serializedKpi = JsonConvert.SerializeObject(kpi);

            var dalKpi = new DalKpi()
            {
                Id = kpi.Id,
                ClassName = kpi.GetType().AssemblyQualifiedName,
                Properties = serializedKpi,
                CreatedDate = kpi.CreatedDate,
                ModifiedDate = kpi.ModifiedDate
            };

            return dalKpi;
        }

        /// <summary>
        /// Takes EF Kpi from the db and deserializes the Json string from properties, then creates/populates the correct kpi class instance.
        /// </summary>
        /// <param name="dalKpi">EF Kpi object.</param>
        /// <returns>User defined Kpi object.</returns>
        private IKpi ConvertToManagerKpi(IDalKpi dalKpi)
        {
            // split up saved assembly/class info
            var parts = dalKpi.ClassName.Split(',');
            
            // create class instance of specific type of kpi
            var kpi = Activator.CreateInstance(parts[1], parts[0]);
            
            // unwrap to actually get the kpi class object so we can populate its properties
            var managerKpi = (IKpi)kpi.Unwrap();
            
            // fill in class properties with values saved to the db
            JsonConvert.PopulateObject(dalKpi.Properties, managerKpi);

            return managerKpi;
        }

    }


}
