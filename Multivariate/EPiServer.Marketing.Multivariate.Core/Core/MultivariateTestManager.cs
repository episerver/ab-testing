using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using EPiServer.Marketing.Multivariate.Dal;
using log4net;


namespace EPiServer.Marketing.Multivariate
{
    public class MultivariateTestManager : IMultivariateTestManager
    {
        internal ILog _log;
        internal IMultivariateTestDal _dataAccess;
        internal ICurrentUser _user;
        internal ICurrentSite _siteData;
        
        private const string _active = "Active";
        private const string _inactive = "Inactive";
        private const string _archived = "Archived";
        private const string _done = "Done";

        public MultivariateTestManager()
        {
            _siteData = new CurrentSite();
            _log = LogManager.GetLogger(typeof(MultivariateTestManager));
            _dataAccess = new MultivariateTestDal(_siteData.GetSiteDataBaseConnectionString());
            _user = new CurrentUser();
        }

        internal MultivariateTestManager(ILog log, IMultivariateTestDal dal, ICurrentUser user, ICurrentSite siteData)
        {
            _log = log;
            _dataAccess = dal;
            _user = user;
            _siteData = siteData;
        }

        public IMultivariateTest Get(Guid testObjectId)
        {
            return ConvertParametersToData(_dataAccess.Get(testObjectId));
        }

        public List<IMultivariateTest> GetTestByItemId(Guid originalItemId)
        {
            return ConvertParametersToData(_dataAccess.GetByOriginalItemId(originalItemId));
        }

        public Guid Save(IMultivariateTest testObject)
        {
            var aParameter = ConvertDataToParameters(testObject);
            aParameter.LastModifiedBy = _user.GetDisplayName();
            aParameter.LastModifiedDate = DateTime.Now;

            var testGuid = aParameter.Id;

            if (testGuid != Guid.Empty)
                _dataAccess.Update(aParameter);
            else
                testGuid = _dataAccess.Add(aParameter);

            return testGuid;
        }

        public void Delete(Guid testObjectId)
        {
            _dataAccess.Delete(testObjectId);
        }

        public void Start(Guid testObjectId)
        {
            var localParameters = _dataAccess.Get(testObjectId);

            if (IsTestActive(localParameters.OriginalItemId))
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
            if (resultType == CountType.View)
                _dataAccess.UpdateViews(testId, testItemId);
            else
                _dataAccess.UpdateConversions(testId, testItemId);
        }


        private void SetTestState(Guid theTestId, TestState theState)
        {
            var aTest = ConvertParametersToData(_dataAccess.Get(theTestId));
            aTest.State = theState;
            Save(aTest);
        }

        private IMultivariateTest ConvertParametersToData(MultivariateTestParameters parameters)
        {
            var aTest = new MultivariateTest();

            aTest.Id = parameters.Id;
            aTest.Title = parameters.Title;
            aTest.Owner = parameters.Owner;
            aTest.State = GetState(parameters.State);
            aTest.OriginalItemId = parameters.OriginalItemId;
            aTest.VariantItemId = parameters.VariantItemId;
            aTest.ConversionItemId = parameters.ConversionItemId;
            aTest.StartDate = parameters.StartDate;
            aTest.EndDate = parameters.EndDate;

            if (parameters.Results != null)
            {
                foreach (var result in parameters.Results)
                {
                    var aResult = new TestResult()
                    {
                        ItemId = result.ItemId,
                        Views = result.Views,
                        Conversions = result.Conversions
                    };
                }
            }

            return aTest;
        }

        private List<IMultivariateTest> ConvertParametersToData(MultivariateTestParameters[] parameters)
        {
            var aRetTests = new List<IMultivariateTest>();

            foreach(MultivariateTestParameters aParam in parameters)
            {
                aRetTests.Add(new MultivariateTest()
                {
                    Id = aParam.Id,
                    Title = aParam.Title,
                    Owner = aParam.Owner,
                    State = GetState(aParam.State),
                    OriginalItemId = aParam.OriginalItemId,
                    VariantItemId = aParam.VariantItemId,
                    ConversionItemId = aParam.ConversionItemId,
                    StartDate = aParam.StartDate,
                    EndDate = aParam.EndDate
                });
            }
            return aRetTests;
        }

        private MultivariateTestParameters ConvertDataToParameters(IMultivariateTest testObject)
        {
            var aRetParameter = new MultivariateTestParameters();

            aRetParameter.Id = testObject.Id;
            aRetParameter.Title = testObject.Title;
            aRetParameter.Owner = testObject.Owner;
            aRetParameter.State = GetStateValue(testObject.State);
            aRetParameter.OriginalItemId = testObject.OriginalItemId;
            aRetParameter.VariantItemId = testObject.VariantItemId;
            aRetParameter.ConversionItemId = testObject.ConversionItemId;
            aRetParameter.StartDate = testObject.StartDate;
            aRetParameter.EndDate = testObject.EndDate;

            return aRetParameter;
        }

        private string GetStateValue(TestState state)
        {
            var retState = string.Empty;
            switch(state)
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
            switch(state)
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
            var allTests = _dataAccess.GetByOriginalItemId(originalItemId);
            return allTests.Any(t => t.State == _active);
        }
    }
}
