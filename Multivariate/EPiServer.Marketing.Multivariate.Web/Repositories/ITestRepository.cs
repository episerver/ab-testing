using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Model;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Multivariate.Web.Models;

namespace EPiServer.Marketing.Multivariate.Web.Repositories
{
    public interface ITestRepository
    {
        Guid CreateTest(ABTestViewModel testData);
        void DeleteTest(Guid testGuid);
        List<ABTestViewModel> GetTestList(TestCriteria criteria);
        ABTestViewModel GetTestById(Guid testId);
        ABTestViewModel ConvertToViewModel(IABTest testToConvert);
        TestResult GetWinningTestResult(ABTestViewModel test);
        void StopTest(Guid tetsGuid);
    }
}
