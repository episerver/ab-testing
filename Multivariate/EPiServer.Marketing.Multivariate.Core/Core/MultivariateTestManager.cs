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

        private const string _active = "Active";
        private const string _inactive = "Inactive";
        private const string _archived = "Archived";
        private const string _done = "Done";

        public MultivariateTestManager()
        {
            _log = LogManager.GetLogger(typeof(MultivariateTestManager));
            _dataAccess = new MultivariateTestDal();
            _user = new CurrentUser();
        }

        internal MultivariateTestManager(ILog log, IMultivariateTestDal dal, ICurrentUser user)
        {
            _log = log;
            _dataAccess = dal;
            _user = user;
        }

        public IMultivariateTest Get(Guid testObjectId)
        {
            return ConvertParametersToData(_dataAccess.Get(testObjectId));
        }

        public IMultivariateTest GetTestByItemId(Guid originalItemId)
        {
            return ConvertParametersToData(_dataAccess.GetTestByPageId(originalItemId));
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
            {
                if(TestExists(aParameter.OriginalItemId))
                {
                    throw new Exception("A test already exists for the given OriginalItemId");
                }
                testGuid = _dataAccess.Add(aParameter);
            }

            return testGuid;
        }

        public void Delete(Guid testObjectId)
        {
            _dataAccess.Delete(testObjectId);
        }

        public void Start(Guid testObjectId)
        {
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

        private void SetTestState(Guid theTestId, TestState theState)
        {
            var aTest = ConvertParametersToData(_dataAccess.Get(theTestId));
            aTest.State = theState;
            Save(aTest);
        }

        private MultivariateTest ConvertParametersToData(MultivariateTestParameters parameters)
        {
            var aRetTest = new MultivariateTest();

            aRetTest.Id = parameters.Id;
            aRetTest.Title = parameters.Title;
            aRetTest.Owner = parameters.Owner;
            aRetTest.State = GetState(parameters.State);
            aRetTest.OriginalItemId = parameters.OriginalItemId;
            aRetTest.VariantItemId = parameters.VariantItemId;
            aRetTest.ConversionItemId = parameters.ConversionItemId;
            aRetTest.StartDate = parameters.StartDate;
            aRetTest.EndDate = parameters.EndDate;

            return aRetTest;
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

        private bool TestExists(Guid originalItemId)
        {
            var test = _dataAccess.GetTestByPageId(originalItemId);
            return test != null;
        }
    }
}
