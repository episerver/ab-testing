using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPiServer.Marketing.Multivariate.Dal;

namespace EPiServer.Marketing.Multivariate.Web.Repositories
{
    public interface IMultivariateTestRepository
    {
        void CreateTest(string title, DateTime testStart, DateTime testStop, int originalPageLink, int variantPageLink, int conversionPageLink);
        List<IMultivariateTest> GetTestList(MultivariateTestCriteria criteria);
    }
}
