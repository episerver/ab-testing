using System;
using System.Collections.Generic;
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

        public IEnumerable<Type> GetAllKpis()
        {
            List<IKpi> KpiList = new List<IKpi>();
            var type = typeof(IKpi);
            // exclude interfaces, abstract instances, and the convience base class Kpi
            var types =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p) && !p.IsInterfaceOrAbstract() && p != typeof(Kpi) );

            //foreach (Type t in types)
            //{
            //    var newKpi = Activator.CreateInstance(t) as IKpi;
            //    KpiList.Add(newKpi);
            //}
            return (types);
        }
             

        /// <summary>
        /// Serialize the kpi to a Json string and save it in the properties field.
        /// </summary>
        /// <param name="kpi">Kpi to save to the db (i.e. contentcomparatorkpi, timeonpagekpi, etc.</param>
        /// <returns>EF Kpi object to save in the db.</returns>
        private IDalKpi ConvertToDalTest(IKpi kpi)
        {
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
