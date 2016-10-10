using System;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Models;
using System.Collections.Generic;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    public interface IMarketingTestingWebRepository
    {
        IMarketingTest GetTestById(Guid testGuid);
        List<IMarketingTest> GetTestList(TestCriteria criteria);
        Guid CreateMarketingTest(TestingStoreModel testData);
        void DeleteMarketingTest(Guid testGuid);
        void StartMarketingTest(Guid testGuid);
        void StopMarketingTest(Guid testGuid);
        void ArchiveMarketingTest(Guid testObjectId, Guid winningVariantId);
        Guid SaveMarketingTest(IMarketingTest testData);
        IMarketingTest GetActiveTestForContent(Guid contentGuid);
        void DeleteTestForContent(Guid contentGuid);
        string PublishWinningVariant(TestResultStoreModel testResult);
    }
}
