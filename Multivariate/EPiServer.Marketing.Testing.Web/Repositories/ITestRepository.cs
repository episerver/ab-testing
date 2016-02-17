using System;
using System.Collections.Generic;
using EPiServer.DataAbstraction;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Web.Models;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    public interface ITestRepository
    {
        Guid CreateTest(ABTestViewModel testData);
        void DeleteTest(Guid testGuid);
        List<ABTestViewModel> GetTestList(TestCriteria criteria);
        ABTestViewModel GetTestById(Guid testId);
        ABTestViewModel ConvertToViewModel(IABTest testToConvert);
        ABTest ConvertToMultivariateTest(ABTestViewModel viewModelToConvert);

        TestResult GetWinningTestResult(ABTestViewModel test);
        void StopTest(Guid tetsGuid);
        PageVersionCollection GetContentVersions(Guid originalPageReference);
    }
}
