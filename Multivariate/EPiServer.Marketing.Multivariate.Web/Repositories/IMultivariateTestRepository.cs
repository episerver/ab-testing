using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Multivariate.Web.Repositories
{
    interface IMultivariateTestRepository
    {
        void CreateTest(string title, DateTime testStart, DateTime testStop, int originalPageLink, int variantPageLink, int conversionPageLink);
    }
}
