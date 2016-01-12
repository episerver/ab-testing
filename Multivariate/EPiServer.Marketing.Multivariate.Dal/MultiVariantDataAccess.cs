using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;

namespace EPiServer.Marketing.Multivariate.Dal
{
    public class MultiVariantDataAccess : IMultiVariantDataAccess
    {
        internal IRepository _repository;

        public MultiVariantDataAccess()
        {
            // TODO : Load repository from service locator.
            _repository = new BaseRepository(new DatabaseContext());
        }
        internal MultiVariantDataAccess(IRepository repository)
        {
            _repository = repository;
        }

        public void Archive(Guid testObjectId)
        {
            SetTestState(testObjectId, TestState.Archived);
        }

        public void Delete(Guid testObjectId)
        {
            _repository.DeleteTest(testObjectId);
            _repository.SaveChanges();
        }

        public IMultivariateTest Get(Guid testObjectId)
        {
            return _repository.GetById(testObjectId);
        }

        public List<IMultivariateTest> GetTestByItemId(Guid originalItemId)
        {
            return _repository.GetAll().Where(t => t.OriginalItemId == originalItemId).ToList();
        }

        public List<IMultivariateTest> GetTestList(MultivariateTestCriteria criteria)
        {
            // TODO:implement criteria object and retrieve accordingly
            return _repository.GetAll().ToList();
        }

        public void IncrementCount(Guid testId, Guid testItemId, CountType resultType)
        {
            throw new NotImplementedException();
        }

        public Guid Save(IMultivariateTest testObject)
        {
            throw new NotImplementedException();
        }

        public void Start(Guid testObjectId)
        {
            var test = _repository.GetById(testObjectId);
            if (IsTestActive(test.OriginalItemId))
            {
                throw new Exception("The test page already has an Active test");
            }

            SetTestState(testObjectId, TestState.Active);
        }

        public void Stop(Guid testObjectId)
        {
            SetTestState(testObjectId, TestState.Done);
        }
        private bool IsTestActive(Guid originalItemId)
        {
            var tests = _repository.GetAll()
                .Where(t => t.OriginalItemId == originalItemId && t.TestState == TestState.Active);

            return tests.Any();
        }
        private void SetTestState(Guid theTestId, TestState theState)
        {
            var aTest = _repository.GetById(theTestId);
            aTest.TestState = theState;
            Save(aTest);
        }
    }
}
