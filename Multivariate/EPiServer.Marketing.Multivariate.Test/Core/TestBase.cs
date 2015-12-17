using System;
using System.Collections.Generic;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;
using EPiServer.Marketing.Multivariate.Test.Dal;

namespace EPiServer.Marketing.Multivariate.Test.Core
{
    public class TestBase
    {
        public void AddObjectsToContext<T>(TestContext context, IList<T> data) where T : class
        {
            context.Set<T>().AddRange(data);
        }

        public IList<MultivariateTest> AddMultivariateTests(TestContext context)
        {
            var newMultivariateTests = new List<MultivariateTest>()
            {
                new MultivariateTest() { Id = Guid.NewGuid(), Title = "test1", CreatedDate = DateTime.UtcNow, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow, TestState = (int)TestState.Active, Owner = "Bert"},
                new MultivariateTest() { Id = Guid.NewGuid(), Title = "test2", CreatedDate = DateTime.UtcNow, StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow, TestState = (int)TestState.Done, Owner = "Ernie"}
            };

            AddObjectsToContext(context, newMultivariateTests);
            return newMultivariateTests;
        }
    }
}
