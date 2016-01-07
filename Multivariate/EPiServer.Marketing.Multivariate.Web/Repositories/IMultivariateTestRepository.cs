using System;
using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Web.Models;

namespace EPiServer.Marketing.Multivariate.Web.Repositories
{
    public interface IMultivariateTestRepository
    {
        Guid CreateTest(MultivariateTestViewModel testData);
        void DeleteTest(Guid testGuid);
        List<IMultivariateTest> GetTestList(MultivariateTestCriteria criteria);
        IMultivariateTest GetTestById(Guid testId);
        MultivariateTestViewModel ConvertToViewModel(IMultivariateTest testToConvert);
        MultivariateTestResult GetWinningTestResult(IMultivariateTest test);
    }
}
