using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EPiServer.Marketing.Testing.Dal.Entity;
using EPiServer.Marketing.Testing.Dal.Entity.Enums;

namespace EPiServer.Marketing.Testing.Dal
{
    internal class TestingDataAccess : ITestingDataAccess
    {
        internal IRepository _repository;

        public TestingDataAccess()
        {
            // TODO : Load repository from service locator.
            _repository = new BaseRepository(new DatabaseContext());
        }
        internal TestingDataAccess(IRepository repository)
        {
            _repository = repository;
        }

        public void Archive(Guid testObjectId)
        {
            SetTestState(testObjectId, TestState.Archived);
        }

        public void Delete(Guid testObjectId)
        {
            _repository.DeleteTest(testObjectId);
            _repository.SaveChanges();
        }

        public IABTest Get(Guid testObjectId)
        {
            return _repository.GetById(testObjectId);
        }

        public List<IABTest> GetTestByItemId(Guid originalItemId)
        {
            return _repository.GetAll().Where(t => t.OriginalItemId == originalItemId).ToList();
        }

        public List<IABTest> GetTestList(TestCriteria criteria)
        {
            // if no filters are passed in, just return all tests
            var filters = criteria.GetFilters();
            if (!filters.Any())
            {
                return _repository.GetAll().ToList();
            }

            var variantOperator = FilterOperator.And;
            IQueryable<IABTest> variantResults = null;
            var variantId = Guid.Empty;
            var pe = Expression.Parameter(typeof(ABTest), "test");
            Expression wholeExpression = null;

            // build up expression tree based on the filters that are passed in
            foreach (var filter in filters)
            {
                // if we are filtering on a single property(not an element in a list) create the expression
                if (filter.Property != ABTestProperty.VariantId)
                {
                    var left = Expression.Property(pe, typeof (ABTest).GetProperty(filter.Property.ToString()));
                    var right = Expression.Constant(filter.Value);
                    var expression = Expression.Equal(left, right);

                    // first time through, so we just set the expression to the first filter criteria and continue to the next one
                    if (wholeExpression == null)
                    {
                        wholeExpression = expression;
                        continue;
                    }

                    // each subsequent iteration we check to see if the filter is for an AND or OR and append accordingly
                    wholeExpression = filter.Operator == FilterOperator.And
                        ? Expression.And(wholeExpression, expression) : Expression.Or(wholeExpression, expression);
                }
                else // if we are filtering on an item in a list, then generate simple results that we can lump in at the end
                {
                    variantId = new Guid(filter.Value.ToString());
                    variantOperator = filter.Operator;
                    variantResults = _repository.GetAll().Where(x => x.Variants.Any(v => v.ItemId == variantId));
                }
            }

            IQueryable<IABTest> results = null;
            var tests = _repository.GetAll().AsQueryable();

            // if we have created an expression tree, then execute it against the tests to get the results
            if (wholeExpression != null)
            {
                var whereCallExpression = Expression.Call(
                    typeof (Queryable),
                    "Where",
                    new Type[] {tests.ElementType},
                    tests.Expression,
                    Expression.Lambda<Func<ABTest, bool>>(wholeExpression, new ParameterExpression[] {pe})
                    );

                results = tests.Provider.CreateQuery<ABTest>(whereCallExpression);
            }

            // if we are also filtering against a variantId, include those results
            if (variantResults != null)
            {
                if (results == null)
                {
                    return variantResults.ToList();
                }

                results = variantOperator == FilterOperator.And 
                    ? results.Where(test => test.Variants.Any(v => v.ItemId == variantId)) 
                    : results.Concat(variantResults).Distinct();
            }

            return results.ToList<IABTest>();
        }


        public void IncrementCount(Guid testId, Guid testItemId, CountType resultType)
        {
            var test = _repository.GetById(testId);
            var result = test.TestResults.FirstOrDefault(v => v.ItemId == testItemId);

            if (resultType == CountType.View)
            {
                result.Views++;
            }
            else
            {
                result.Conversions++;
            }

            _repository.SaveChanges();
        }

        public Guid Save(IABTest testObject)
        {
            var id = testObject.Id;

            // if a test doesn't exist, add it to the db
            var test = _repository.GetById(testObject.Id) as ABTest;
            if (test == null)
            {
                _repository.Add(testObject);
            }
            else
            {
                if (test.State == TestState.Inactive)
                {
                    test.Title = testObject.Title;
                    test.OriginalItemId = testObject.OriginalItemId;
                    test.LastModifiedBy = testObject.LastModifiedBy;
                    test.StartDate = testObject.StartDate;
                    test.EndDate = testObject.EndDate;
                    test.ModifiedDate = DateTime.UtcNow;

                    // remove any existing kpis that are not part of the new test
                    foreach (var existingKpi in test.KeyPerformanceIndicators.ToList())
                    {
                        if (testObject.KeyPerformanceIndicators.All(k => k.Id != existingKpi.Id))
                        {
                            _repository.Delete(existingKpi);
                        }
                    }

                    // update existing kpis that are still around and add any that are new
                    foreach (var newKpi in testObject.KeyPerformanceIndicators)
                    {
                        var existingKpi = test.KeyPerformanceIndicators.SingleOrDefault(k => k.Id == newKpi.Id);

                        if (existingKpi != null)
                        {
                            existingKpi.KeyPerformanceIndicatorId = newKpi.KeyPerformanceIndicatorId;
                            existingKpi.ModifiedDate = DateTime.UtcNow;
                        }
                        else
                        {
                            test.KeyPerformanceIndicators.Add(newKpi);
                        }
                    }

                    // remove any existing results that are not part of the new test
                    foreach (var existingResult in test.TestResults.ToList())
                    {
                        if (testObject.TestResults.All(k => k.Id != existingResult.Id))
                        {
                            _repository.Delete(existingResult);
                        }
                    }

                    // update existing results that are still around and add any that are new
                    foreach (var newResult in testObject.TestResults)
                    {
                        var existingResult = test.TestResults.SingleOrDefault(k => k.Id == newResult.Id);

                        if (existingResult != null)
                        {
                            existingResult.Conversions = newResult.Conversions;
                            existingResult.Views = newResult.Views;
                            existingResult.ItemId = newResult.ItemId;
                            existingResult.ItemVersion = newResult.ItemVersion;
                            existingResult.ModifiedDate = DateTime.UtcNow;
                        }
                        else
                        {
                            test.TestResults.Add(newResult);
                        }
                    }

                    // remove any existing variants that are not part of the new test
                    foreach (var existingVariant in test.Variants.ToList())
                    {
                        if (testObject.Variants.All(k => k.Id != existingVariant.Id))
                        {
                            _repository.Delete(existingVariant);
                        }
                    }

                    // update existing variants that are still around and add any that are new
                    foreach (var newVariant in testObject.Variants)
                    {
                        var existingVariant = test.Variants.SingleOrDefault(k => k.Id == newVariant.Id);

                        if (existingVariant != null)
                        {
                            existingVariant.ItemId = newVariant.ItemId;
                            existingVariant.ItemVersion = newVariant.ItemVersion;
                            existingVariant.ModifiedDate = DateTime.UtcNow;
                        }
                        else
                        {
                            test.Variants.Add(newVariant);
                        }
                    }
                }
            }

            _repository.SaveChanges();

            return id;
        }

        public void Start(Guid testObjectId)
        {
            var test = _repository.GetById(testObjectId);
            if (IsTestActive(test.OriginalItemId))
            {
                throw new Exception("The test page already has an Active test");
            }

            SetTestState(testObjectId, TestState.Active);
        }

        public void Stop(Guid testObjectId)
        {
            SetTestState(testObjectId, TestState.Done);
        }
        private bool IsTestActive(Guid originalItemId)
        {
            var tests = _repository.GetAll()
                .Where(t => t.OriginalItemId == originalItemId && t.State == TestState.Active);

            return tests.Any();
        }
        private void SetTestState(Guid theTestId, TestState theState)
        {
            var aTest = _repository.GetById(theTestId);
            aTest.State = theState;
            _repository.SaveChanges();
        }
    }
}
