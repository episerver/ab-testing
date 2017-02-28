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
    /// <inheritdoc />
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

        /// <inheritdoc />
        public IKpi Get(Guid kpiId)
        {
            return ConvertToManagerKpi(_dataAccess.Get(kpiId));
        }

        /// <inheritdoc />
        public List<IKpi> GetKpiList()
        {
            return _dataAccess.GetKpiList().Select(dalTest => ConvertToManagerKpi(dalTest)).ToList();
        }

        /// <inheritdoc />
        public Guid Save(IKpi kpi)
        {
            return _dataAccess.Save(ConvertToDalKpi(kpi));
        }

        /// <inheritdoc />
        public void Delete(Guid kpiId)
        {
            _dataAccess.Delete(kpiId);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void SaveCommerceSettings(CommerceData commerceSettings)
        {            
            var store = GetDataStore(typeof(CommerceData));
            store.Save(commerceSettings);
        }

        /// <inheritdoc />
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
        private IDalKpi ConvertToDalKpi(IKpi kpi)
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
