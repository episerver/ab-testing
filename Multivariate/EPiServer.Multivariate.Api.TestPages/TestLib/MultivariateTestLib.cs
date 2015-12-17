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


            MultivariateTestManager mtm = new MultivariateTestManager();
            List<IMultivariateTest> discoveredTests = new List<IMultivariateTest>();
            IMultivariateTestRepository testRepo = new MultivariateTestRepository();

            ICurrentSite currentSite = new CurrentSite();

            discoveredTests = mtm.GetTestList(new MultivariateTestCriteria());



            return discoveredTests;
        }

        private List<KeyPerformanceIndicator> Kpis;
        private Guid originalItemGuid;
        private List<Variant> variantsToSave;



        public Guid CreateAbTest(MultivariateTest dataToSave)
        {

            _mtm = new MultivariateTestManager();




            if (dataToSave.Id == Guid.Empty)
            {
                Kpis = new List<KeyPerformanceIndicator>()
                {
                    new KeyPerformanceIndicator()
                    {
                      KeyPerformanceIndicatorId  = Guid.NewGuid()
                    },

                };
                originalItemGuid = Guid.NewGuid();
                variantsToSave = new List<Variant>()
                {
                    new Variant()
                    {
                        VariantId = dataToSave.Variants[0].VariantId
                    }
                };


            }
            else
            {

                originalItemGuid = dataToSave.OriginalItemId;

            }

            var savedTest = new MultivariateTest
            {
                KeyPerformanceIndicators = Kpis,

                EndDate = dataToSave.EndDate,
                OriginalItemId = originalItemGuid,
                StartDate = dataToSave.StartDate,
                Title = dataToSave.Title,
                Variants = variantsToSave,
                Owner = dataToSave.Owner,
                MultivariateTestResults = new List<MultivariateTestResult>()
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
            MultivariateTest currentTest = (MultivariateTest)_mtm.Get(testId);
            _mtm.Start(testId);
            for (int x = 0; x < 5; x++)
            {
                currentTest.MultivariateTestResults.Add(new MultivariateTestResult()
                {
                    ItemId = _mtm.ReturnLandingPage(testId)
                });

            }


            foreach (var result in currentTest.MultivariateTestResults)
            {
                for (int views = 0; views < currentTest.MultivariateTestResults.Count(); views++)
                {
                    _mtm.IncrementCount(testId, result.ItemId, views%5 == 0 ? CountType.Conversion : CountType.View);
                }
            }

            _mtm.Stop(testId);


            return _mtm.Get(testId);

        }
    }




}