using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using EPiServer.Marketing.Multivariate;

namespace EpiAutomation.TestDevelopment.Testing
{
    public class MultivariateTestLib
    {
        private MultivariateTestManager _mtm;
        private DateTime _today = DateTime.Now;

        public List<IMultivariateTest> GetTests()
        {
            List<IMultivariateTest> discoveredTests = new List<IMultivariateTest>();
            ICurrentSite currentSite = new CurrentSite();
            using (SqlConnection sqlConnection = new SqlConnection(currentSite.GetSiteDataBaseConnectionString()))
            {
                string query = "SELECT * FROM tblMultivariateTests";
                using (SqlCommand command = new SqlCommand(query, sqlConnection))
                {
                    sqlConnection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            discoveredTests.Add(new MultivariateTest
                            {
                                Id = reader.GetGuid(0),
                                Title = reader.GetString(1),
                                Owner = reader.GetString(2),
                                OriginalItemId = reader.GetGuid(3),
                                VariantItemId = reader.GetGuid(4),
                                ConversionItemId = reader.GetGuid(5),
                                State = (TestState)Enum.Parse(typeof(TestState), reader.GetString(6)),
                                StartDate = reader.GetDateTime(7),
                                EndDate = reader.GetDateTime(8),
                                Results = new List<TestResult>()
                                
                            });
                            
                        }
                    }
                }

            }

            using (SqlConnection sqlConnection = new SqlConnection(currentSite.GetSiteDataBaseConnectionString()))
            {
                sqlConnection.Open();
                foreach (var test in discoveredTests)
                {
                    
                    string query = string.Format("SELECT * FROM tblMultivariateTestsResults WHERE TestId='{0}'",test.Id);
                    using (SqlCommand command = new SqlCommand(query, sqlConnection))
                    {
                       
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                test.Results.Add(new TestResult
                                {
                                    ItemId = reader.GetGuid(2),
                                    Views = reader.GetInt32(3),
                                    Conversions = reader.GetInt32(4)
                                });
                                

                            }
                           
                        }
                        
                    }
                }

            }
            

            return discoveredTests;
        }

       

        /// <summary>
        /// Saves a multivariate test
        /// todo
        ///  Remove hardcoded test data and add parameter for
        ///  test supplied test data
        /// </summary>
        /// <returns>Guid of the saved test</returns>
        public Guid SaveAbTest()
        {
            _mtm = new MultivariateTestManager();

            Guid testItemId = Guid.NewGuid();

            Guid savedTest = _mtm.Save(new MultivariateTest
            {
                ConversionItemId = Guid.NewGuid(),
                EndDate = _today.AddDays(3),
                OriginalItemId = testItemId,
                StartDate = _today,
                Title = "SaveAbTest",
                VariantItemId = Guid.NewGuid(),
                Owner = "API Testing"
            });

            _mtm.IncrementCount(savedTest, testItemId, CountType.View);
            _mtm.IncrementCount(savedTest, testItemId, CountType.Conversion);
            _mtm.IncrementCount(savedTest, testItemId, CountType.View);

            return savedTest;
        }

        public Guid SaveAbTest(MultivariateTest dataToSave)
        {

            _mtm = new MultivariateTestManager();

            Guid testItemId = Guid.NewGuid();

            var savedTest = _mtm.Save(new MultivariateTest
            {
                ConversionItemId = Guid.NewGuid(),
                EndDate = dataToSave.EndDate,
                OriginalItemId = testItemId,
                StartDate = dataToSave.StartDate,
                Title = dataToSave.Title,
                VariantItemId = Guid.NewGuid(),
                Owner = dataToSave.Owner,
                
               
            });

            _mtm.IncrementCount(savedTest,testItemId,CountType.View);
            _mtm.IncrementCount(savedTest, testItemId, CountType.Conversion);
            _mtm.IncrementCount(savedTest, testItemId, CountType.View);

            return savedTest;
        }



        /// <summary>
        /// Gets a specified multiviate test
        /// 
        /// todo
        ///     Remove hardcoded test data and add parametr for
        ///     test supplied test data
        /// </summary>
        /// <returns>Instance of IMultivariateTest containing the retrieved test information</returns>
        public MultivariateTest GetAbTest()
        {
            _mtm = new MultivariateTestManager();

            Guid testItemId = Guid.NewGuid();

            Guid savedTest = _mtm.Save(new MultivariateTest
            {
                ConversionItemId = Guid.NewGuid(),
                EndDate = _today.AddDays(3),
                OriginalItemId = testItemId,
                StartDate = _today,
                Title = "First AB Test",
                VariantItemId = Guid.NewGuid(),
                Owner = "AutoTest",
            
            });

            _mtm.IncrementCount(savedTest, testItemId, CountType.View);
            _mtm.IncrementCount(savedTest, testItemId, CountType.Conversion);
            _mtm.IncrementCount(savedTest, testItemId, CountType.View);

            //Could combine these two into one statement but we may want
            //to excplicitly check for null and act up on it.
            IMultivariateTest returnedTest = _mtm.Get(savedTest);
            return returnedTest as MultivariateTest;
        }

        /// <summary>
        /// Gets a list of tests associated with the provided ID
        /// 
        /// todo
        ///     Remove hardcoded test data and add parametr for
        ///     test supplied test data
        /// </summary>
        /// <returns>List of IMultivariateTests containing</returns>
        public List<IMultivariateTest> GetAbTestList()
        {
            _mtm = new MultivariateTestManager();

            Guid testItemId = Guid.NewGuid();

            Guid savedTest = _mtm.Save(new MultivariateTest
            {
                ConversionItemId = Guid.NewGuid(),
                EndDate = _today.AddDays(3),
                OriginalItemId = testItemId,
                StartDate = _today,
                Title = "First AB Test",
                VariantItemId = Guid.NewGuid(),
                Owner = "AutoTest",

            });

            _mtm.IncrementCount(savedTest, testItemId, CountType.View);
            _mtm.IncrementCount(savedTest, testItemId, CountType.Conversion);
            _mtm.IncrementCount(savedTest, testItemId, CountType.View);

            //Could combine these two into one statement but we may want
            //to excplicitly check for null and act up on it.
            List<IMultivariateTest> returnedTestList = _mtm.GetTestByItemId(testItemId);
            return returnedTestList;
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


        public IMultivariateTest RunTest()
        {
            _mtm = new MultivariateTestManager();
            Guid mockItemId = Guid.NewGuid();

            Guid testGuid = _mtm.Save(new MultivariateTest
            {
                ConversionItemId = Guid.NewGuid(),
                EndDate = _today.AddSeconds(10),
                OriginalItemId = mockItemId,
                StartDate = _today,
                Title = "First AB Test",
                VariantItemId = Guid.NewGuid(),
                Owner = "AutoTest",
            });

            Thread.Sleep(20000);

            IMultivariateTest returnedTest = _mtm.Get(testGuid);
            return returnedTest;

        }

        
    }
}