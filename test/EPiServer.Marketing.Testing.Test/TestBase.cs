using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Test.Dal;
using EPiServer.Marketing.Testing.Dal.DataAccess;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;

namespace EPiServer.Marketing.Testing.Test
{
    public class TestBase
    {
        public void AddObjectsToContext<T>(TestContext context, IList<T> data) where T : class
        {
            context.Set<T>().AddRange(data);
        }

        public IList<DalABTest> AddMultivariateTests(TestContext context, int numberOfTests)
        {
            var newMultivariateTests = new List<DalABTest>();

            for (var i = 0; i < numberOfTests; i++)
            {
                newMultivariateTests.Add(new DalABTest()
                {
                    Id = Guid.NewGuid(),
                    Title = "test" + i,
                    Description = "description" + i,
                    CreatedDate = DateTime.UtcNow,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    LastModifiedBy = "me",
                    Owner = "Bert" + i,
                    OriginalItemId = Guid.NewGuid(),
                    State = DalTestState.Inactive,
                    ParticipationPercentage = 100,
                    AutoPublishWinner = false,
                    Variants = new List<DalVariant>(),
                    KeyPerformanceIndicators = new List<DalKeyPerformanceIndicator>()
                });
            };

            AddObjectsToContext(context, newMultivariateTests);
            context.SaveChanges();
            return newMultivariateTests;
        }

        internal IList<DalABTest> AddMultivariateTests(TestingDataAccess mtmManager, int numberOfTests)
        {
            var newABTests = new List<DalABTest>();

            for (var i = 0; i < numberOfTests; i++)
            {
                var test = new DalABTest()
                {
                    Id = Guid.NewGuid(),
                    Title = "test" + i,
                    Description = "description" + i,
                    CreatedDate = DateTime.UtcNow,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow,
                    State = DalTestState.Inactive,
                    ParticipationPercentage = 100,
                    LastModifiedBy = "me",
                    OriginalItemId = Guid.NewGuid(),
                    Owner = "Bert" + i,
                    AutoPublishWinner = true,
                    Variants = new List<DalVariant>(),
                    KeyPerformanceIndicators = new List<DalKeyPerformanceIndicator>()
                };

                mtmManager.Save(test);
                newABTests.Add(test);
            }

            return newABTests;
        }
    }
}
