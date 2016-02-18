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

        public void Delete(Guid kpiObjectId)
        {
            _repository.DeleteKpi(kpiObjectId);
            _repository.SaveChanges();
        }

        public Kpi Get(Guid kpiObjectId)
        {
            return _repository.GetById(kpiObjectId);
        }

        public List<Kpi> GetKpiByItemId(Guid originalItemId)
        {
            return _repository.GetAll().ToList();
        }

        public List<Kpi> GetKpiList()
        {
            return _repository.GetAll().ToList();
        }

        public Guid Save(Kpi kpiObject)
        {
            var kpi = _repository.GetById(kpiObject.Id) as Kpi;
            Guid id;

            // if a test doesn't exist, add it to the db
            //if (kpi == null)
            {
                _repository.Add(kpiObject);
                id = kpiObject.Id;
            }
 

            _repository.SaveChanges();

            return id;
        }

    }
}
