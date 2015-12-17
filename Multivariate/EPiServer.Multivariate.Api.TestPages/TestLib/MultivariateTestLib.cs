using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Marketing.Multivariate;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Model.Enums;
using EPiServer.Marketing.Multivariate.Web.Repositories;
using NuGet;

namespace EPiServer.Multivariate.Api.TestPages.TestLib
{
    public class MultivariateTestLib
    {
        private MultivariateTestManager _mtm;

        public List<IMultivariateTest> GetTests()
        {



            List<IMultivariateTest> discoveredTests = new List<IMultivariateTest>();
            IMultivariateTestRepository testRepo = new MultivariateTestRepository();

            ICurrentSite currentSite = new CurrentSite();

            discoveredTests = testRepo.GetTestList(new MultivariateTestCriteria());



            return discoveredTests;
        }

        private List<KeyPerformanceIndicator> conversionsToSave;
        private Guid originalItemGuid;
        private List<Variant> variantsToSave;



        public Guid CreateAbTest(MultivariateTest dataToSave)
        {

            _mtm = new MultivariateTestManager();




            if (dataToSave.Id == Guid.Empty)
            {
                conversionsToSave = new List<KeyPerformanceIndicator>()
                {
                    new KeyPerformanceIndicator(),

                };
                originalItemGuid = Guid.NewGuid();
                variantsToSave = new List<Variant>()
                {
                    new Variant()
                };


            }
            else
            {
                dataToSave.KeyPerformanceIndicators.AddRange(conversionsToSave);

                originalItemGuid = dataToSave.OriginalItemId;
                dataToSave.Variants.AddRange(variantsToSave);

            }

            var savedTest = new MultivariateTest
            {
                Id = Guid.NewGuid(),
                KeyPerformanceIndicators = conversionsToSave,
                
                EndDate = dataToSave.EndDate,
                OriginalItemId = originalItemGuid,
                StartDate = dataToSave.StartDate,
                Title = dataToSave.Title,
                Variants = variantsToSave,
                Owner = dataToSave.Owner,
            };

            _mtm.Save(savedTest);

            return savedTest.Id;
        }

        /// <summary>
        /// Gets a list of tests associated with the provided ID
        /// 
        /// todo
        ///     Remove hardcoded test data and add parametr for
        ///     test supplied test data
        /// </summary>
        /// <returns>List of IMultivariateTests containing</returns>
        public List<IMultivariateTest> GetAbTestList(string originalItemId)
        {
            var itemId = new Guid(originalItemId);
            _mtm = new MultivariateTestManager();

            return _mtm.GetTestList(new MultivariateTestCriteria()).Where(t => t.OriginalItemId == itemId).ToList();
        }


        public IMultivariateTest SetAbState(Guid testId, TestState? state)
        {
            _mtm = new MultivariateTestManager();
            switch (state)
            {
                case TestState.Active:
                    _mtm.Start(testId);
                    break;
                case TestState.Inactive:
                    _mtm.Stop(testId);
                    break;
                case TestState.Archived:
                    _mtm.Archive(testId);
                    break;
                default:
                    return null;

            }
            return _mtm.Get(testId);
        }





        public IMultivariateTest RunTests(Guid testId)
        {
            _mtm = new MultivariateTestManager();
            List<Guid> results = new List<Guid>();
            _mtm.Start(testId);
            for (int x = 0; x < 1000; x++)
            {

                results.Add(_mtm.ReturnLandingPage(testId));
            }

            var pageResults = results.GroupBy(x => x);
            foreach (var result in pageResults)
            {
                for (int views = 0; views < result.Count(); views++)
                {
                    if (views % 5 == 0)
                        _mtm.IncrementCount(testId, result.Key, CountType.Conversion);
                    else
                        _mtm.IncrementCount(testId, result.Key, CountType.View);
                }
            }

            _mtm.Stop(testId);


            return _mtm.Get(testId);

        }
    }




}