using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.DataAbstraction;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.KPI.Dal.Model;

namespace EPiServer.Marketing.KPI.DataAccess
{
    internal class KpiDataAccess : IKpiDataAccess
    {
        internal IRepository _repository;
        internal bool _UseEntityFramework;

        public KpiDataAccess()
        {
            _UseEntityFramework = true;
            // TODO : Load repository from service locator.
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
            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    DeleteHelper(repository, kpiId);
                }
            }
            else
            {
                DeleteHelper(_repository, kpiId);
            }
        }

        private void DeleteHelper(IRepository repo, Guid kpiId)
        {
            repo.DeleteKpi(kpiId);
            repo.SaveChanges();
        }

        /// <summary>
        /// Returns a KPI object based on its Id.
        /// </summary>
        /// <param name="kpiId">Id of the KPI to retrieve.</param>
        /// <returns>KPI object.</returns>
        public IDalKpi Get(Guid kpiId)
        {
            IDalKpi kpi;

            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    kpi = GetHelper(repository, kpiId);
                }
            }
            else
            {
                kpi = GetHelper(_repository, kpiId);
            }

            return kpi;
        }

        private IDalKpi GetHelper(IRepository repo, Guid kpiId)
        {
            return repo.GetById(kpiId);
        }

        /// <summary>
        /// Gets the whole list of KPI objects.
        /// </summary>
        /// <returns>List of KPI objects.</returns>
        public List<IDalKpi> GetKpiList()
        {
            List<IDalKpi> kpis;

            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    kpis = GetKpiListHelper(repository);
                }
            }
            else
            {
                kpis = GetKpiListHelper(_repository);
            }

            return kpis;
        }

        private List<IDalKpi> GetKpiListHelper(IRepository repo)
        {
            return repo.GetAll().ToList();
        }

        /// <summary>
        /// Adds or updates a KPI object.
        /// </summary>
        /// <param name="kpiObject">Id of the KPI to add/update.</param>
        /// <returns>The Id of the KPI object that was added/updated.</returns>
        public Guid Save(IDalKpi kpiObject)
        {
            Guid id;

            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    id = SaveHelper(repository, kpiObject);
                }
            }
            else
            {
                id = SaveHelper(_repository, kpiObject);
            }

            return id;
        }


        public Guid SaveHelper(IRepository repo, IDalKpi kpiObject)
        {
            var kpi = repo.GetById(kpiObject.Id) as DalKpi;
            Guid id;

            // if a test doesn't exist, add it to the db
            if (kpi == null)
            {
                repo.Add(kpiObject);
                id = kpiObject.Id;
            }
            else
            {
                kpi.ClassName = kpiObject.ClassName;
                kpi.Properties = kpiObject.Properties;
                id = kpi.Id;
            }

            repo.SaveChanges();

            return id;
        }
    }
}
