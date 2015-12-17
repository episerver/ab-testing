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
        internal IRepository _repository;
        internal ICurrentUser _user;
        internal ICurrentSite _siteData;

        private const string _active = "Active";
        private const string _inactive = "Inactive";
        private const string _archived = "Archived";
        private const string _done = "Done";

        private static Random _r = new Random();

        public MultivariateTestManager()
        {
            _siteData = new CurrentSite();
            _user = new CurrentUser();
            _repository = new BaseRepository(new DatabaseContext());
        }

        internal MultivariateTestManager(ICurrentUser user, ICurrentSite siteData, IRepository repo)
        {
            _user = user;
            _siteData = siteData;
            _repository = repo;
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

        /// <summary>
        /// Add new test to the db or updated an existing test if it already exists.
        /// </summary>
        /// <param name="multivariateTest"></param>
        /// <returns>Id of the new or modified test.</returns>
        public Guid Save(IMultivariateTest multivariateTest)
        {
            var test = _repository.GetById(multivariateTest.Id);
            Guid id;

            if (test == null)
            {
                _repository.Add(multivariateTest);
                id = multivariateTest.Id;
            }
            else
            {
                test.Title = multivariateTest.Title;
                test.StartDate = multivariateTest.StartDate;
                test.EndDate = multivariateTest.EndDate;
                test.Owner = multivariateTest.Owner;
                test.LastModifiedBy = multivariateTest.LastModifiedBy;
                test.ModifiedDate = DateTime.UtcNow;
                test.Conversions = multivariateTest.Conversions;
                test.Variants = multivariateTest.Variants;
                test.KeyPerformanceIndicators = multivariateTest.KeyPerformanceIndicators;
                test.MultivariateTestResults = multivariateTest.MultivariateTestResults;
                id = test.Id;
            }

            _repository.SaveChanges();

            return id;
        }

        public void Delete(Guid testObjectId)
        {
            _repository.DeleteTest(testObjectId);
            _repository.SaveChanges();
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

        public void Archive(Guid testObjectId)
        {
            SetTestState(testObjectId, TestState.Archived);
        }

        public void IncrementCount(Guid testId, Guid testItemId, CountType resultType)
        {
            var test = _repository.GetById(testId);
            var result = test.MultivariateTestResults.FirstOrDefault(v => v.ItemId == testItemId);

            if (resultType == CountType.View)
            {
                result.Views++;
            }
            else
            {
                result.Conversions++;
            }
        }

        public Guid ReturnLandingPage(Guid testId)
        {
            var currentTest = _repository.GetById(testId);
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

        private void SetTestState(Guid theTestId, TestState theState)
        {
            var aTest = _repository.GetById(theTestId);
            aTest.TestState = (int)theState;
            Save(aTest);
        }

        private string GetStateValue(TestState state)
        {
            var retState = string.Empty;
            switch (state)
            {
                case TestState.Active:
                    retState = _active;
                    break;
                case TestState.Inactive:
                    retState = _inactive;
                    break;
                case TestState.Done:
                    retState = _done;
                    break;
                case TestState.Archived:
                    retState = _archived;
                    break;
            }
            return retState;
        }

        private TestState GetState(string state)
        {
            switch (state)
            {
                case _active:
                    return TestState.Active;
                case _archived:
                    return TestState.Archived;
                case _done:
                    return TestState.Done;
                default:
                    return TestState.Inactive;
            }
        }

        private bool IsTestActive(Guid originalItemId)
        {
            var tests = _repository.GetAll()
                .Where(t => t.OriginalItemId == originalItemId && t.TestState == (int)TestState.Active);

            return tests.Any();
        }

        // This is only a placeholder. This will be replaced by a method which uses a more structured algorithm/formula
        // to determine what page to display to the user.
        private int GetRandomNumber()
        {
            return _r.Next(1, 3);
        }
    
    }
}
