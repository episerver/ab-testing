using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Dal.Entity;
using EPiServer.Marketing.Multivariate.Test.Dal;
using EPiServer.Marketing.Testing.Dal;

namespace EPiServer.Marketing.Testing.Test
{
    public class TestBase
    {
        public void AddObjectsToContext<T>(TestContext context, IList<T> data) where T : class
        {
            context.Set<T>().AddRange(data);
        }

        public IList<ABTest> AddMultivariateTests(TestContext context, int numberOfTests)
        {
            var newMultivariateTests = new List<ABTest>();

            for (var i = 0; i < numberOfTests; i++)
            {
                newMultivariateTests.Add(new ABTest()
                {
                    Id = Guid.NewGuid(),
                    Title = "test" + i,
                    CreatedDate = DateTime.UtcNow,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    Owner = "Bert" + i,
                    TestResults = new List<TestResult>(),
                    Variants = new List<Variant>(),
                });
            };

            AddObjectsToContext(context, newMultivariateTests);
            context.SaveChanges();
            return newMultivariateTests;
        }

        public IList<ABTest> AddMultivariateTests(TestingDataAccess mtmManager, int numberOfTests)
        {
            var newABTests = new List<ABTest>();

            for (var i = 0; i < numberOfTests; i++)
            {
                var test = new ABTest()
                {
                    Id = Guid.NewGuid(),
                    Title = "test" + i,
                    CreatedDate = DateTime.UtcNow,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    Owner = "Bert" + i,
                    TestResults = new List<TestResult>(),
                    Variants = new List<Variant>(),
                };

                mtmManager.Save(test);
                newABTests.Add(test);
            }

            return newABTests;
        }

        public void AddMultivariateTestResults(TestingDataAccess mtmManager, ABTest multivariateTest, Guid itemId)
        {
            var result = new TestResult()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Views = 0,
                Conversions = 0,
                ItemId = itemId
            };

            multivariateTest.TestResults.Add(result);
            mtmManager.Save(multivariateTest);
        }
    }
}
