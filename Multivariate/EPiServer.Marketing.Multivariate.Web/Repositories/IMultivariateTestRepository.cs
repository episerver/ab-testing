using System;
using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Multivariate.Web.Models;

namespace EPiServer.Marketing.Multivariate.Web.Repositories
{
    public interface IMultivariateTestRepository
    {
        Guid CreateTest(MultivariateTestViewModel testData);
        void DeleteTest(Guid testGuid);
        List<MultivariateTestViewModel> GetTestList(MultivariateTestCriteria criteria);
        MultivariateTestViewModel GetTestById(Guid testId);
        MultivariateTestViewModel ConvertToViewModel(IMultivariateTest testToConvert);
        MultivariateTestResult GetWinningTestResult(MultivariateTestViewModel test);
        void StopTest(Guid tetsGuid);
    }
}
