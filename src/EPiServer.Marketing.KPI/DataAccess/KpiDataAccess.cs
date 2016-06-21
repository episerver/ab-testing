using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.DataAccess
{
    internal class KpiDataAccess : IKpiDataAccess
    {
        internal IRepository _repository;

        public KpiDataAccess()
        {
            // TODO : Load repository from service locator.
            _repository = new BaseRepository(new DatabaseContext());
        }
        internal KpiDataAccess(IRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Deletes KPI object from the db.
        /// </summary>
        /// <param name="kpiId">Id of the KPI to delete.</param>
        public void Delete(Guid kpiId)
        {
            _repository.DeleteKpi(kpiId);
            _repository.SaveChanges();
        }

        /// <summary>
        /// Returns a KPI object based on its Id.
        /// </summary>
        /// <param name="kpiId">Id of the KPI to retrieve.</param>
        /// <returns>KPI object.</returns>
        public IDalKpi Get(Guid kpiId)
        {
            return _repository.GetById(kpiId);
        }

        /// <summary>
        /// Gets the whole list of KPI objects.
        /// </summary>
        /// <returns>List of KPI objects.</returns>
        public List<IDalKpi> GetKpiList()
        {
            return _repository.GetAll().ToList();
        }

        /// <summary>
        /// Adds or updates a KPI object.
        /// </summary>
        /// <param name="kpiObject">Id of the KPI to add/update.</param>
        /// <returns>The Id of the KPI object that was added/updated.</returns>
        public Guid Save(IDalKpi kpiObject)
        {
            var kpi = _repository.GetById(kpiObject.Id) as DalKpi;
            Guid id;

            // if a test doesn't exist, add it to the db
            if (kpi == null)
            {
                _repository.Add(kpiObject);
                id = kpiObject.Id;
            }
            else
            {
                kpi.ClassName = kpiObject.ClassName;
                kpi.Properties = kpiObject.Properties;
                id = kpi.Id;
            }

            _repository.SaveChanges();

            return id;
        }

    }
}
