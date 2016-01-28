using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;
using System.Reflection;

namespace EPiServer.Marketing.Multivariate.Dal
{
    public class MultiVariantDataAccess : IMultiVariantDataAccess
    {
        internal IRepository _repository;

        public MultiVariantDataAccess()
        {
            // TODO : Load repository from service locator.
            _repository = new BaseRepository(new DatabaseContext());
        }
        internal MultiVariantDataAccess(IRepository repository)
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

        public IMultivariateTest Get(Guid testObjectId)
        {
            return _repository.GetById(testObjectId);
        }

        public List<IMultivariateTest> GetTestByItemId(Guid originalItemId)
        {
            return _repository.GetAll().Where(t => t.OriginalItemId == originalItemId).ToList();
        }

        public List<IMultivariateTest> GetTestList(MultivariateTestCriteria criteria)
        {
            // if no filters are passed in, just return all tests
            var filters = criteria.GetFilters();
            if (!filters.Any())
            {
                return _repository.GetAll().ToList();
            }

            var variantOperator = FilterOperator.And;
            IQueryable<IMultivariateTest> variantResults = null;
            var variantId = Guid.Empty;
            var pe = Expression.Parameter(typeof(MultivariateTest), "test");
            Expression wholeExpression = null;

            // build up expression tree based on the filters that are passed in
            foreach (var filter in filters)
            {
                // if we are filtering on a single property(not an element in a list) create the expression
                if (filter.Property != MultivariateTestProperty.VariantId)
                {
                    var left = Expression.Property(pe, typeof (MultivariateTest).GetProperty(filter.Property.ToString()));
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
                    variantResults = _repository.GetAll().Where(x => x.Variants.Any(v => v.VariantId == variantId));
                }
            }

            IQueryable<IMultivariateTest> results = null;
            var tests = _repository.GetAll().AsQueryable();

            // if we have created an expression tree, then execute it against the tests to get the results
            if (wholeExpression != null)
            {
                var whereCallExpression = Expression.Call(
                    typeof (Queryable),
                    "Where",
                    new Type[] {tests.ElementType},
                    tests.Expression,
                    Expression.Lambda<Func<MultivariateTest, bool>>(wholeExpression, new ParameterExpression[] {pe})
                    );

                results = tests.Provider.CreateQuery<MultivariateTest>(whereCallExpression);
            }

            // if we are also filtering against a variantId, include those results
            if (variantResults != null)
            {
                if (results == null)
                {
                    return variantResults.ToList();
                }

                results = variantOperator == FilterOperator.And 
                    ? results.Where(test => test.Variants.Any(v => v.VariantId == variantId)) 
                    : results.Concat(variantResults).Distinct();
            }

            return results.ToList<IMultivariateTest>();
        }


        public void IncrementCount(Guid testId, Guid testItemId, CountType resultType)
        {
            var test = _repository.GetById(testId);
            var result = test.MultivariateTestResults.FirstOrDefault(v => v.ItemId == testItemId);

            if (resultType == CountType.View)
            {
                result.Views++;
            }
            else
            {
                result.Conversions++;
            }

            Save(test);
        }

        public Guid Save(IMultivariateTest testObject)
        {
            var test = _repository.GetById(testObject.Id);
            Guid id;

            if (test == null)
            {
                _repository.Add(testObject);
                id = testObject.Id;
            }
            else
            {
                test.Title = testObject.Title;
                test.StartDate = testObject.StartDate;
                test.EndDate = testObject.EndDate;
                test.Owner = testObject.Owner;
                test.LastModifiedBy = testObject.LastModifiedBy;
                test.ModifiedDate = DateTime.UtcNow;
                test.Conversions = testObject.Conversions;
                test.Variants = testObject.Variants;
                test.KeyPerformanceIndicators = testObject.KeyPerformanceIndicators;
                test.MultivariateTestResults = testObject.MultivariateTestResults;
                id = test.Id;
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
                .Where(t => t.OriginalItemId == originalItemId && t.TestState == TestState.Active);

            return tests.Any();
        }
        private void SetTestState(Guid theTestId, TestState theState)
        {
            var aTest = _repository.GetById(theTestId);
            aTest.TestState = theState;
            Save(aTest);
        }
    }
}
