using System;
using System.Collections.Generic;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Data.Enums;

namespace EPiServer.Marketing.Testing
{
    public interface ITestManager
    {
        IMarketingTest Get(Guid testObjectId);

        List<IMarketingTest> GetTestByItemId(Guid originalItemId);

        List<IMarketingTest> GetTestList(TestCriteria criteria);

        Guid Save(IMarketingTest testObject);

        void Delete(Guid testObjectId);

        void Start(Guid testObjectId);

        void Stop(Guid testObjectId);

        void Archive(Guid testObjectId);

        void IncrementCount(Guid testId, Guid testItemId, int itemVersion, CountType resultType);

        void EmitUpdateCount(Guid testId, Guid testItemId, int itemVersion, CountType resultType);

        Variant ReturnLandingPage(Guid testId);
        PageData CreateVariantPageDataCache(Guid contentGuid, List<ContentReference> processedList);
        List<IMarketingTest> CreateActiveTestCache();
    }
}
