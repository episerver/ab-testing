using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EPiServer.Marketing.KPI.Dal.Model;
using EPiServer.Marketing.KPI.DataAccess;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using StructureMap.TypeRules;

namespace EPiServer.Marketing.KPI.Manager
{
    [ServiceConfiguration(ServiceType = typeof(IKpiManager), Lifecycle = ServiceInstanceScope.Singleton)]
    public class KpiManager : IKpiManager
    {
        private IKpiDataAccess _dataAccess;
        private IServiceLocator _serviceLocator;

        [ExcludeFromCodeCoverage]
        public KpiManager()
        {
            _serviceLocator = ServiceLocator.Current;
            _dataAccess = new KpiDataAccess();
        }

        internal KpiManager(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _dataAccess = _serviceLocator.GetInstance<IKpiDataAccess>();
        }

        public IKpi Get(Guid kpiId)
        {
            return ConvertToManagerKpi(_dataAccess.Get(kpiId));
        }

        public List<IKpi> GetKpiList()
        {
            return _dataAccess.GetKpiList().Select(dalTest => ConvertToManagerKpi(dalTest)).ToList();
        }

        public Guid Save(IKpi kpi)
        {
            return _dataAccess.Save(ConvertToDalTest(kpi));
        }

        public void Delete(Guid kpiId)
        {
            _dataAccess.Delete(kpiId);
        }

        public IEnumerable<Type> GetKpiTypes()
        {
            var type = typeof(IKpi);
            // exclude interfaces, abstract instances, and the convience base class Kpi
            var types =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p) && !p.IsInterfaceOrAbstract() && p != typeof(Kpi) );
            return (types);
        }

        public long GetDatabaseVersion(DbConnection dbConnection, string schema, string contextKey)
        {
            return _dataAccess.GetDatabaseVersion(dbConnection, schema, contextKey);
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
