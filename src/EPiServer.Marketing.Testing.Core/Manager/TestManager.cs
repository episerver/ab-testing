using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Exceptions;
using EPiServer.Marketing.Testing.Dal.DataAccess;
using EPiServer.Marketing.Testing.Dal.Exceptions;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// Central point of access for test data and test manipulation.
    /// </summary>
    public class TestManager : ITestManager
    {        
        private ITestingDataAccess _dataAccess;
        private IServiceLocator _serviceLocator;
        private Random _randomParticiaption = new Random();
        private IKpiManager _kpiManager;
        private DefaultMarketingTestingEvents _marketingTestingEvents;
        private bool _databaseNeedsConfiguring;

        [ExcludeFromCodeCoverage]
        public TestManager()
        {
            _serviceLocator = ServiceLocator.Current;
            _marketingTestingEvents = _serviceLocator.GetInstance<DefaultMarketingTestingEvents>();

            try
            {
                _dataAccess = new TestingDataAccess();
            }
            catch (DatabaseDoesNotExistException)
            {
                _databaseNeedsConfiguring = true;
                return;
            }
            catch (DatabaseNeedsUpdating)
            {
                _databaseNeedsConfiguring = true;
                return;
            }

            _kpiManager = new KpiManager();
        }

        internal TestManager(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _dataAccess = _serviceLocator.GetInstance<ITestingDataAccess>();
            _kpiManager = _serviceLocator.GetInstance<IKpiManager>();
            _marketingTestingEvents = _serviceLocator.GetInstance<DefaultMarketingTestingEvents>();
        }

        /// <inheritdoc />
        public IMarketingTest Get(Guid testObjectId, bool fromCachedTests = false)
        {
            return TestManagerHelper.ConvertToManagerTest(_kpiManager, _dataAccess.Get(testObjectId)) 
                ?? throw new TestNotFoundException();
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetActiveTests()
        {
            var allActiveTests = new TestCriteria();
            allActiveTests.AddFilter(
                new ABTestFilter
                {
                    Property = ABTestProperty.State,
                    Operator = FilterOperator.And, 
                    Value = TestState.Active
                }
            );

            return GetTestList(allActiveTests);
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId)
        {
            return GetTestByItemId(originalItemId).Where(t => t.State == TestState.Active).ToList();
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId,CultureInfo contentCulture)
        {
            return GetTestByItemId(originalItemId).Where(t => t.State == TestState.Active && t.ContentLanguage == contentCulture.Name).ToList();
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetTestByItemId(Guid originalItemId)
        {
            var testList = new List<IMarketingTest>();

            foreach (var dalTest in _dataAccess.GetTestByItemId(originalItemId))
            {
                testList.Add(TestManagerHelper.ConvertToManagerTest(_kpiManager, dalTest));
            }

            return testList;
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetTestList(TestCriteria criteria)
        {
            var testList = new List<IMarketingTest>();

            foreach (var dalTest in _dataAccess.GetTestList(TestManagerHelper.ConvertToDalCriteria(criteria)))
            {
                testList.Add(TestManagerHelper.ConvertToManagerTest(_kpiManager, dalTest));
            }

            return testList;
        }

        /// <summary>
        /// Saves a test to the database.
        /// </summary>
        /// <param name="multivariateTest">A test.</param>
        /// <returns>ID of the test.</returns>
        public Guid Save(IMarketingTest multivariateTest)
        {
            if (multivariateTest.KpiInstances == null)
            {
                throw new SaveTestException("Unable to save test due to null list of KPI's.  One or more KPI's are required.");
            }

            if (multivariateTest.KpiInstances.Count == 0)
            {
                throw new SaveTestException("Unable to save test due to empty list of KPI's.  One or more KPI's are required.");
            }

            var testId = _dataAccess.Save(TestManagerHelper.ConvertToDalTest(multivariateTest));

            _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestSavedEvent, new TestEventArgs(multivariateTest));

            if (multivariateTest.State == TestState.Active)
            {
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStartedEvent, new TestEventArgs(multivariateTest));
            }

            return testId;
        }

        /// <inheritdoc />
        public void Delete(Guid testObjectId, CultureInfo cultureInfo = null)
        {
            var testToDelete = Get(testObjectId);            

            foreach (var kpi in testToDelete.KpiInstances)
            {
                _kpiManager.Delete(kpi.Id);
            }

            _dataAccess.Delete(testObjectId);

            _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestDeletedEvent, new TestEventArgs(testToDelete));
        }
       
        /// <inheritdoc />
        public IMarketingTest Start(Guid testObjectId)
        {
            var dalTest = _dataAccess.Start(testObjectId);
            var managerTest = TestManagerHelper.ConvertToManagerTest(_kpiManager, dalTest);

            if (dalTest != null)
            {
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStartedEvent, new TestEventArgs(managerTest));
            }

            return managerTest;
        }

        /// <inheritdoc />
        public void Stop(Guid testObjectId, CultureInfo cultureInfo = null)
        {
            _dataAccess.Stop(testObjectId);

            var stoppedTest = Get(testObjectId);            
            
            if (stoppedTest != null)
            {                
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStoppedEvent, new TestEventArgs(stoppedTest));
            }
        }

        /// <inheritdoc />
        public void Archive(Guid testObjectId, Guid winningVariantId, CultureInfo cultureInfo = null)
        {
            _dataAccess.Archive(testObjectId, winningVariantId);

            var archivedTest = Get(testObjectId);
            if (archivedTest != null)
            {
                _marketingTestingEvents.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestArchivedEvent, new TestEventArgs(archivedTest));
            }
        }

        /// <inheritdoc />
        public void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type, bool aSynch = true)
        {
            if (aSynch)
            {
                var messaging = _serviceLocator.GetInstance<IMessagingManager>();
                messaging.EmitKpiResultData(testId, itemVersion, keyResult, type);
            }
            else
            {
                switch (type)
                {
                    case KeyResultType.Financial:
                        _dataAccess.AddKpiResultData(testId, itemVersion, TestManagerHelper.ConvertToDalKeyFinancialResult((KeyFinancialResult)keyResult), (int)type);
                        break;
                    case KeyResultType.Value:
                        _dataAccess.AddKpiResultData(testId, itemVersion,
                            TestManagerHelper.ConvertToDalKeyValueResult((KeyValueResult) keyResult), (int) type);
                        break;
                    case KeyResultType.Conversion:
                    default:
                        _dataAccess.AddKpiResultData(testId, itemVersion,
                            TestManagerHelper.ConvertToDalKeyConversionResult((KeyConversionResult)keyResult), (int)type);
                        break;
                }
            }
        }

        /// <inheritdoc />
        public Variant ReturnLandingPage(Guid testId)
        {
            var currentTest = _dataAccess.Get(testId);
            var managerTest = TestManagerHelper.ConvertToManagerTest(_kpiManager, currentTest);
            var activePage = new Variant();
            if (managerTest != null)
            {
                if (_randomParticiaption.Next(1, 100) <= managerTest.ParticipationPercentage)
                {
                    switch (TestManagerHelper.GetRandomNumber())
                    {
                        case 1:
                        default:
                            activePage = TestManagerHelper.ConvertToManagerVariant(currentTest.Variants[0]);
                            break;
                        case 2:
                            activePage = TestManagerHelper.ConvertToManagerVariant(currentTest.Variants[1]);
                            break;
                    }

                    _marketingTestingEvents.RaiseMarketingTestingEvent(
                        DefaultMarketingTestingEvents.UserIncludedInTestEvent, 
                        new TestEventArgs(managerTest)
                    );
                }
            }
            return activePage;
        }

        /// <inheritdoc />
        public IContent GetVariantContent(Guid contentGuid)
        {
            return GetVariantContent(contentGuid, new CultureInfo("en-GB"));
        }

        /// <inheritdoc />
        public IContent GetVariantContent(Guid contentGuid, CultureInfo cultureInfo)
        {
            IVersionable variantContent = null;

            var test = GetActiveTestsByOriginalItemId(contentGuid, cultureInfo).FirstOrDefault(x => x.State.Equals(TestState.Active));

            if (test != null)
            {
                var contentLoader = _serviceLocator.GetInstance<IContentLoader>();
                var testContent = contentLoader.Get<IContent>(contentGuid);

                if (testContent != null)
                {
                    var contentVersion = testContent.ContentLink.WorkID == 0
                        ? test.Variants.First(v => v.IsPublished).ItemVersion
                        : testContent.ContentLink.WorkID;

                    var variant = test.Variants.Where(v => v.ItemVersion != contentVersion).FirstOrDefault();

                    if (variant != null)
                    {
                        variantContent = (IVersionable)TestManagerHelper.CreateVariantContent(contentLoader, testContent, variant);
                        variantContent.Status = VersionStatus.Published;
                        variantContent.StartPublish = DateTime.Now.AddDays(-1);
                    }
                }
            }

            return (IContent)variantContent;
        }

        private Object _incrementLock = new Object();
        /// <inheritdoc />
        public void IncrementCount(IncrementCountCriteria criteria)
        {
            if (criteria.asynch)
            {
                var messaging = _serviceLocator.GetInstance<IMessagingManager>();
                if (criteria.resultType == CountType.Conversion)
                    messaging.EmitUpdateConversion(criteria.testId, criteria.itemVersion, criteria.kpiId, criteria.clientId);
                else if (criteria.resultType == CountType.View)
                    messaging.EmitUpdateViews(criteria.testId, criteria.itemVersion);
            }
            else
            {
                lock (_incrementLock)
                {
                    _dataAccess.IncrementCount(criteria.testId, criteria.itemVersion, TestManagerHelper.AdaptToDalCount(criteria.resultType), criteria.kpiId);
                }
            }
        }

        /// <inheritdoc />
        public void IncrementCount(Guid testId, int itemVersion, CountType resultType, Guid kpiId = default(Guid), bool asynch = true)
        {
            var c = new IncrementCountCriteria()
            {
                testId = testId,
                itemVersion = itemVersion,
                resultType = resultType,
                kpiId = kpiId,
                asynch = asynch
            };
            IncrementCount(c);
        }

        /// <inheritdoc />
        public IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e)
        {
            return kpis.Select(kpi => kpi.Evaluate(sender, e)).ToList();
        }

        /// <inheritdoc />
        public long GetDatabaseVersion(DbConnection dbConnection, string schema, string contextKey, bool populateCache = false)
        {
            if (_databaseNeedsConfiguring)
            {
                _databaseNeedsConfiguring = false;
                return 0;
            }

            if (populateCache)
            {
                _dataAccess = new TestingDataAccess();
                _kpiManager = new KpiManager();
            }

            return _dataAccess.GetDatabaseVersion(dbConnection, schema, contextKey);
        }
    }
}
