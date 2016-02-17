using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Marketing.Testing.KPI.Dal;
using EPiServer.Marketing.Testing.KPI.Model;

namespace EPiServer.Marketing.Testing.KPI.DataAccess
{
    public class KpiDataAccess : IKpiDataAccess
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
        public IKpi Get(Guid kpiId)
        {
            return _repository.GetById(kpiId);
        }

        /// <summary>
        /// Gets the whole list of KPI objects.
        /// </summary>
        /// <returns>List of KPI objects.</returns>
        public List<IKpi> GetKpiList()
        {
            return _repository.GetAll().ToList();
        }

        /// <summary>
        /// Adds or updates a KPI object.
        /// </summary>
        /// <param name="kpiObject">Id of the KPI to add/update.</param>
        /// <returns>The Id of the KPI object that was added/updated.</returns>
        public Guid Save(IKpi kpiObject)
        {
            var kpi = _repository.GetById(kpiObject.Id) as Kpi;
            Guid id;

            // if a test doesn't exist, add it to the db
            if (kpi == null)
            {
                _repository.Add(kpiObject);
                id = kpiObject.Id;
            }
            else
            {
                kpi.Name = kpiObject.Name;
                kpi.Value = kpiObject.Value;

                id = kpi.Id;
            }

            _repository.SaveChanges();

            return id;
        }

    }
}
