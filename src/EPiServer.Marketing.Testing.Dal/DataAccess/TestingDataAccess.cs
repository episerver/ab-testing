using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Dal.EntityModel.Enums;

namespace EPiServer.Marketing.Testing.Dal.DataAccess
{
    internal class TestingDataAccess : ITestingDataAccess
    {
        internal IRepository _repository;
        internal bool _UseEntityFramework;

        public TestingDataAccess()
        {
            _UseEntityFramework = true;
            // TODO : Load repository from service locator.
        }
        internal TestingDataAccess(IRepository repository)
        {
            _repository = repository;
        }

        public void Archive(Guid testObjectId)
        {
            SetTestState(testObjectId, DalTestState.Archived);
        }

        public void Delete(Guid testObjectId)
        {
            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    DeleteHelper(repository, testObjectId);
                }
            }
            else
            {
                DeleteHelper(_repository, testObjectId);
            }

        }

        public IABTest Get(Guid testObjectId)
        {
            IABTest test;

            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    test = repository.GetById(testObjectId);
                }
            }
            else
            {
                test = _repository.GetById(testObjectId);
            }

            return test;
        }

        // TODO : rename to GetTestsByItemId
        public List<IABTest> GetTestByItemId(Guid originalItemId)
        {
            List<IABTest> tests;

            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    tests = repository.GetAll().Where(t => t.OriginalItemId == originalItemId).ToList();
                }
            }
            else
            {
                tests = _repository.GetAll().Where(t => t.OriginalItemId == originalItemId).ToList();
            }

            return tests;
        }

        public List<IABTest> GetTestList(DalTestCriteria criteria)
        {
            List<IABTest> tests;

            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    tests = GetTestListHelper(repository, criteria);
                }
            }
            else
            {
                tests = GetTestListHelper(_repository, criteria);
            }

            return tests;
        }

        public void IncrementCount(Guid testId, Guid testItemId, int itemVersion, DalCountType resultType)
        {
            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    IncrementCountHelper(repository, testId, testItemId, itemVersion, resultType);
                }
            }
            else
            {
                IncrementCountHelper(_repository, testId, testItemId, itemVersion, resultType);
            }
        }

        public Guid Save(IABTest testObject)
        {
            Guid id;

            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    id = SaveHelper(repository, testObject);
                }
            }
            else
            {
                id = SaveHelper(_repository, testObject);
            }

            return id;
        }

        public IABTest Start(Guid testObjectId )
        {
            if (IsTestActive(testObjectId))
            {
                throw new Exception("The test page already has an Active test");
            }

            return SetTestState(testObjectId, DalTestState.Active);
        }

        public void Stop(Guid testObjectId)
        {
            SetTestState(testObjectId, DalTestState.Done);
        }



        #region Private Helpers
        private void DeleteHelper(IRepository repo, Guid testId)
        {
            repo.DeleteTest(testId);
            repo.SaveChanges();
        }

        private List<IABTest> GetTestListHelper(IRepository repo, DalTestCriteria criteria)
        {
            // if no filters are passed in, just return all tests
            var filters = criteria.GetFilters();
            if (!filters.Any())
            {
                return repo.GetAll().ToList();
            }

            var variantOperator = DalFilterOperator.And;
            IQueryable<IABTest> variantResults = null;
            var variantId = Guid.Empty;
            var pe = Expression.Parameter(typeof(DalABTest), "test");
            Expression wholeExpression = null;

            // build up expression tree based on the filters that are passed in
            foreach (var filter in filters)
            {
                // if we are filtering on a single property(not an element in a list) create the expression
                if (filter.Property != DalABTestProperty.VariantId)
                {
                    var left = Expression.Property(pe, typeof(DalABTest).GetProperty(filter.Property.ToString()));
                    var right = Expression.Constant(filter.Value);
                    var expression = Expression.Equal(left, right);

                    // first time through, so we just set the expression to the first filter criteria and continue to the next one
                    if (wholeExpression == null)
                    {
                        wholeExpression = expression;
                        continue;
                    }

                    // each subsequent iteration we check to see if the filter is for an AND or OR and append accordingly
                    wholeExpression = filter.Operator == DalFilterOperator.And
                        ? Expression.And(wholeExpression, expression)
                        : Expression.Or(wholeExpression, expression);
                }
                else
                // if we are filtering on an item in a list, then generate simple results that we can lump in at the end
                {
                    variantId = new Guid(filter.Value.ToString());
                    variantOperator = filter.Operator;
                    variantResults = repo.GetAll().Where(x => x.Variants.Any(v => v.ItemId == variantId));
                }
            }

            IQueryable<IABTest> results = null;
            var tests = repo.GetAll().AsQueryable();

            // if we have created an expression tree, then execute it against the tests to get the results
            if (wholeExpression != null)
            {
                var whereCallExpression = Expression.Call(
                    typeof(Queryable),
                    "Where",
                    new Type[] { tests.ElementType },
                    tests.Expression,
                    Expression.Lambda<Func<DalABTest, bool>>(wholeExpression, new ParameterExpression[] { pe })
                    );

                results = tests.Provider.CreateQuery<DalABTest>(whereCallExpression);
            }

            // if we are also filtering against a variantId, include those results
            if (variantResults != null)
            {
                if (results == null)
                {
                    return variantResults.ToList();
                }

                results = variantOperator == DalFilterOperator.And
                    ? results.Where(test => test.Variants.Any(v => v.ItemId == variantId))
                    : results.Concat(variantResults).Distinct();
            }

            return results.ToList<IABTest>();
        }

        private void IncrementCountHelper(IRepository repo, Guid testId, Guid testItemId, int itemVersion, DalCountType resultType)
        {
            var test = repo.GetById(testId);
            var variant = test.Variants.FirstOrDefault(v => v.ItemId == testItemId && v.ItemVersion == itemVersion);

            if (resultType == DalCountType.View)
            {
                variant.Views++;
            }
            else
            {
                variant.Conversions++;
            }

            repo.SaveChanges();
        }

        private Guid SaveHelper(IRepository repo, IABTest testObject)
        {
            var id = testObject.Id;

            // if a test doesn't exist, add it to the db
            var test = repo.GetById(testObject.Id) as DalABTest;
            if (test == null)
            {
                repo.Add(testObject);
            }
            else
            {
                switch (test.State)
                {
                    case DalTestState.Inactive:
                    test.Title = testObject.Title;
                    test.Description = testObject.Description;
                    test.OriginalItemId = testObject.OriginalItemId;
                    test.LastModifiedBy = testObject.LastModifiedBy;
                    test.StartDate = testObject.StartDate.ToUniversalTime();
                    if(testObject.EndDate.HasValue)
                    {
                        test.EndDate = testObject.EndDate.Value.ToUniversalTime();
                    }
                    else
                    {
                    test.EndDate = testObject.EndDate;
                    }

                    test.ModifiedDate = DateTime.UtcNow;
                        test.ParticipationPercentage = testObject.ParticipationPercentage;

                    // remove any existing kpis that are not part of the new test
                    foreach (var existingKpi in test.KeyPerformanceIndicators.ToList())
                    {
                        if (testObject.KeyPerformanceIndicators.All(k => k.Id != existingKpi.Id))
                        {
                            repo.Delete(existingKpi);
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

                    // remove any existing variants that are not part of the new test
                    foreach (var existingVariant in test.Variants.ToList())
                    {
                        if (testObject.Variants.All(k => k.Id != existingVariant.Id))
                        {
                            repo.Delete(existingVariant);
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
                            existingVariant.Views = newVariant.Views;
                            existingVariant.Conversions = newVariant.Conversions;
                                existingVariant.IsWinner = newVariant.IsWinner;
                        }
                        else
                        {
                            test.Variants.Add(newVariant);
                        }
                    }
                        break;
                    case DalTestState.Done:
                        test.State = testObject.State == DalTestState.Archived ? DalTestState.Archived : DalTestState.Done;
                        test.IsSignificant = testObject.IsSignificant;
                        test.ZScore = testObject.ZScore;
                        test.ModifiedDate = DateTime.UtcNow;
                        break;
                    case DalTestState.Active:
                        test.State = testObject.State;
                        test.ModifiedDate = DateTime.UtcNow;
                        break;
                }
            }
            repo.SaveChanges();

            return id;
        }

        private bool IsTestActive(Guid testId)
        {
            bool isActive;

            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    isActive = IsTestActiveHelper(repository, testId);
                }
            }
            else
            {
                isActive = IsTestActiveHelper(_repository, testId);
            }

            return isActive;
        }

        private bool IsTestActiveHelper(IRepository repo, Guid testId)
        {
            var test = repo.GetById(testId);
            var tests = repo.GetAll()
                        .Where(t => t.OriginalItemId == test.OriginalItemId && t.State == DalTestState.Active);

            return tests.Any();
        }

        private IABTest SetTestState(Guid theTestId, DalTestState theState)
        {
            IABTest test;

            if (_UseEntityFramework)
            {
                using (var dbContext = new DatabaseContext())
                {
                    var repository = new BaseRepository(dbContext);
                    test = SetTestStateHelper(repository, theTestId, theState);
                }
            }
            else
            {
                test = SetTestStateHelper(_repository, theTestId, theState);
            }

            return test;
        }

        private IABTest SetTestStateHelper(IRepository repo, Guid theTestId, DalTestState theState)
        {
            var aTest = repo.GetById(theTestId);
            aTest.State = theState;
            repo.SaveChanges();
            return aTest;
        }
        #endregion
    }
}
