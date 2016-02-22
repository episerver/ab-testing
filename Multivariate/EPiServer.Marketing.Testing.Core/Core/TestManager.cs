using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Model.Enums;
using EPiServer.Marketing.Testing.Messaging;

namespace EPiServer.Marketing.Testing
{
    [ServiceConfiguration(ServiceType = typeof(ITestManager), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TestManager : ITestManager
    {
        private ITestingDataAccess _dataAccess;
        private IServiceLocator _serviceLocator;
        private static Random _r = new Random();

        [ExcludeFromCodeCoverage]
        public TestManager()
        {
            _serviceLocator = ServiceLocator.Current;
            _dataAccess = new TestingDataAccess();
        }
        internal TestManager(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _dataAccess = _serviceLocator.GetInstance<ITestingDataAccess>();
        }

        public IABTest Get(Guid testObjectId)
        {
            return _dataAccess.Get(testObjectId);
        }

        public List<IABTest> GetTestByItemId(Guid originalItemId)
        {
            return _dataAccess.GetTestByItemId(originalItemId);
        }

        public List<IABTest> GetTestList(TestCriteria criteria)
        {
            return _dataAccess.GetTestList(criteria);
        }
        public Guid Save(IABTest multivariateTest)
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
                        activePage = currentTest.Variants[0].Id;
                        break;
                    case 2:
                        activePage = currentTest.Variants[1].Id;
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

        public void EmitUpdateCount(Guid testId, Guid testItemId, CountType resultType)
        {
            var messaging = _serviceLocator.GetInstance<IMessagingManager>();
            if (resultType == CountType.Conversion)
                messaging.EmitUpdateConversion(testId, testItemId);
            else if (resultType == CountType.View)
                messaging.EmitUpdateViews(testId, testItemId);
        }
    }
}
