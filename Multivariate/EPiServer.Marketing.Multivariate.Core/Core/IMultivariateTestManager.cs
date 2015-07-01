using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Multivariate
{
    public interface IMultivariateTestManager
    {
        IMultivariateTest Get(Guid testObjectId);
        List<IMultivariateTest> GetTestByItemId(Guid originalItemId);
        Guid Save(IMultivariateTest testObject);
        void Delete(Guid testObjectId);
        void Start(Guid testObjectId);
        void Stop(Guid testObjectId);
        void Archive(Guid testObjectId);
        void IncrementCount(Guid testId, Guid testItemId, CountType resultType);

    }
}
