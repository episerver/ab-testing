using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;

namespace EPiServer.Marketing.Multivariate
{
    [ServiceConfiguration(ServiceType = typeof(IMultivariateTestManager))]
    public class MultivariateTestManager : IMultivariateTestManager
    {
        private IMultiVariantDataAccess _dataAccess;
        private IServiceLocator _serviceLocator;
        private static Random _r = new Random();

        public MultivariateTestManager()
        {
            _serviceLocator = ServiceLocator.Current;
            _dataAccess = new MultiVariantDataAccess();
        }
        internal MultivariateTestManager(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _dataAccess = _serviceLocator.GetInstance<IMultiVariantDataAccess>();
        }

        public IMultivariateTest Get(Guid testObjectId)
        {
            return _dataAccess.Get(testObjectId);
        }

        public List<IMultivariateTest> GetTestByItemId(Guid originalItemId)
        {
            return _dataAccess.GetTestByItemId(originalItemId);
        }

        public List<IMultivariateTest> GetTestList(MultivariateTestCriteria criteria)
        {
            return _dataAccess.GetTestList(criteria);
        }
        public Guid Save(IMultivariateTest multivariateTest)
        {
            // Todo : We should probably check to see if item quid is empty or null and
            // create a new unique guid here?
            // 
            return _dataAccess.Save(multivariateTest);
        }

        public void Delete(Guid testObjectId)
        {
            _dataAccess.Delete(testObjectId);
        }

        public void Start(Guid testObjectId)
        {
            _dataAccess.Start(testObjectId);
        }

        public void Stop(Guid testObjectId)
        {
            _dataAccess.Stop(testObjectId);
        }

        public void Archive(Guid testObjectId)
        {
            _dataAccess.Archive(testObjectId);
        }

        public void IncrementCount(Guid testId, Guid testItemId, CountType resultType)
        {
            _dataAccess.IncrementCount(testId, testItemId, resultType);
        }

        public Guid ReturnLandingPage(Guid testId)
        {
            var currentTest = _dataAccess.Get(testId);
            var activePage = Guid.Empty;

            if (currentTest != null)
            {
                switch (GetRandomNumber())
                {
                    case 1:
                    default:
                        activePage = currentTest.OriginalItemId;
                        break;
                    case 2:
                        activePage = currentTest.Variants[0].VariantId;
                        break;
                }
            }

            return activePage;
        }

        // This is only a placeholder. This will be replaced by a method which uses a more structured algorithm/formula
        // to determine what page to display to the user.
        private int GetRandomNumber()
        {
            return _r.Next(1, 3);
        }
    }
}
