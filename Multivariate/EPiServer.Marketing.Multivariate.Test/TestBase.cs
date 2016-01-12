using System;
using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;
using EPiServer.Marketing.Multivariate.Test.Dal;
using EPiServer.Marketing.Multivariate.Dal;

namespace EPiServer.Marketing.Multivariate.Test.Core
{
    public class TestBase
    {
        public void AddObjectsToContext<T>(TestContext context, IList<T> data) where T : class
        {
            context.Set<T>().AddRange(data);
        }

        public IList<MultivariateTest> AddMultivariateTests(TestContext context, int numberOfTests)
        {
            var newMultivariateTests = new List<MultivariateTest>();

            for (var i = 0; i < numberOfTests; i++)
            {
                newMultivariateTests.Add(new MultivariateTest()
                {
                    Id = Guid.NewGuid(),
                    Title = "test" + i,
                    CreatedDate = DateTime.UtcNow,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    Owner = "Bert" + i
                });
            };

            AddObjectsToContext(context, newMultivariateTests);
            context.SaveChanges();
            return newMultivariateTests;
        }

        public IList<MultivariateTest> AddMultivariateTests(MultiVariantDataAccess mtmManager, int numberOfTests)
        {
            var newMultivariateTests = new List<MultivariateTest>();

            for (var i = 0; i < numberOfTests; i++)
            {
                var test = new MultivariateTest()
                {
                    Id = Guid.NewGuid(),
                    Title = "test" + i,
                    CreatedDate = DateTime.UtcNow,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow,
                    Owner = "Bert" + i,
                    MultivariateTestResults = new List<MultivariateTestResult>(),
                    Variants = new List<Variant>(),
                    Conversions = new List<Conversion>()
                };

                mtmManager.Save(test);
                newMultivariateTests.Add(test);
            }

            return newMultivariateTests;
        }

        public void AddMultivariateTestResults(MultiVariantDataAccess mtmManager, MultivariateTest multivariateTest, Guid itemId)
        {
            var result = new MultivariateTestResult()
            {
                Id = Guid.NewGuid(),
                CreatedDate = DateTime.UtcNow,
                Views = 0,
                Conversions = 0,
                ItemId = itemId
            };

            multivariateTest.MultivariateTestResults.Add(result);
            mtmManager.Save(multivariateTest);
        }
    }
}
