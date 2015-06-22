using System;
using System.Collections.Generic;
using System.Linq;

namespace EPiServer.Marketing.Multivariate.Dal
{
    public interface IMultivariateTestDal
    {
        MultivariateTestParameters Get(Guid objectId);
        MultivariateTestParameters GetTestByPageId(Guid originalPageId);
        Guid Add(MultivariateTestParameters parameters);
        bool Update(MultivariateTestParameters parameters);
        bool Delete(Guid Id);
    }
}
