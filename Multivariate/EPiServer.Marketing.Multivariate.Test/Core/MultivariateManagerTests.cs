using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EPiServer.Marketing.Multivariate.Dal;
using System.Collections.Generic;

namespace EPiServer.Marketing.Multivariate.Test.Core
{
    [TestClass]
    public class MultivariateManagerTests
    {
        private Mock<IMultivariateTestDal> dal;
        private Mock<ICurrentUser> user;
        private Mock<IMultivariateTest> testData;
        private Mock<ICurrentSite> siteData;
        private string testUsername = "TestUser";
        private MultivariateTestManager GetUnitUnderTest()
        {
            dal = new Mock<IMultivariateTestDal>();
            user = new Mock<ICurrentUser>();
            siteData = new Mock<ICurrentSite>();
            testData = new Mock<IMultivariateTest>();

            user.Setup(u => u.GetDisplayName()).Returns(testUsername);
            return new MultivariateTestManager(dal.Object, user.Object, siteData.Object);
        }

        //[TestMethod]
        //public void TestManager_Construction_Creates_Internal_Objects()
        //{
        //    var aTestManager = new MultivariateTestManager();

        //    Assert.IsNotNull(aTestManager._log, "The logger should be created upon construction");
        //    Assert.IsNotNull(aTestManager._dataAccess, "The data access object should be created upon construction");
        //    Assert.IsNotNull(aTestManager._user, "The current user object should be created upon construction");
        //}

        [TestMethod]
        public void Save_Adds_A_New_Test_Given_No_Id()
        {
            var testManager = GetUnitUnderTest();
            testData.SetupGet(td => td.Id).Returns(null);

            var testGuid = Guid.NewGuid();
            dal.Setup(d => d.Add(It.IsAny<MultivariateTestParameters>())).Returns(testGuid);
            var retId = testManager.Save(testData.Object);

            Assert.AreEqual(testGuid, retId, "The value returned was not equal to the expected Guid");
            dal.Verify(d => d.Add(It.IsAny<MultivariateTestParameters>()), Times.Once, "Add should only be called once");
        }

        [TestMethod]
        public void Save_Updates_A_Test_With_A_Given_Id()
        {
            var testManager = GetUnitUnderTest();
            testData.SetupGet(td => td.Id).Returns(Guid.NewGuid());

            dal.Setup(d => d.Update(It.IsAny<MultivariateTestParameters>()));
            var retId = testManager.Save(testData.Object);

            dal.Verify(d => d.Update(It.IsAny<MultivariateTestParameters>()), Times.Once, "Update should only be called once");
        }

        [TestMethod]
        public void Delete_Passes_The_Id_Of_The_Item_To_Delete()
        {
            var testManager = GetUnitUnderTest();
            var testGuid = Guid.NewGuid();

            dal.Setup(d => d.Delete(It.IsAny<Guid>()));

            testManager.Delete(testGuid);

            dal.Verify(d => d.Delete(It.IsAny<Guid>()), Times.Once, "Delete should be called once");
            dal.Verify(d => d.Delete(It.Is<Guid>(arg => arg == testGuid)), "The id passed in should be the same as the one passed to the DAL");
        }

        [TestMethod]
        public void Start_Saves_The_Test_State_To_Active()
        {
            var testManager = GetUnitUnderTest();
            var testGuid = Guid.NewGuid();
            dal.Setup(d => d.Update(It.IsAny<MultivariateTestParameters>()));
            dal.Setup(d => d.Get(It.IsAny<Guid>())).Returns(new MultivariateTestParameters() { Id = testGuid });

            testManager.Start(testGuid);

            dal.Verify(d => d.Update(It.Is<MultivariateTestParameters>(arg => arg.Id == testGuid && arg.State == "Active")));
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Start_Throws_When_There_Is_A_Test_Active()
        {
            var testManager = GetUnitUnderTest();
            var testGuid = Guid.NewGuid();
            dal.Setup(d => d.Get(It.IsAny<Guid>())).Returns(new MultivariateTestParameters());
            dal.Setup(d => d.GetByOriginalItemId(It.IsAny<Guid>())).Returns(new MultivariateTestParameters[1] { new MultivariateTestParameters() { State = "Active" } });

            testManager.Start(testGuid);
        }


        [TestMethod]
        public void Stop_Sets_The_Test_State_To_Done()
        {
            var testManager = GetUnitUnderTest();
            var testGuid = Guid.NewGuid();
            dal.Setup(d => d.Update(It.IsAny<MultivariateTestParameters>()));
            dal.Setup(d => d.Get(It.IsAny<Guid>())).Returns(new MultivariateTestParameters() { Id = testGuid });

            testManager.Stop(testGuid);

            dal.Verify(d => d.Update(It.Is<MultivariateTestParameters>(arg => arg.Id == testGuid && arg.State == "Done")));
        }

        [TestMethod]
        public void Archive_Sets_The_Test_State_To_Archived()
        {
            var testManager = GetUnitUnderTest();
            var testGuid = Guid.NewGuid();
            dal.Setup(d => d.Update(It.IsAny<MultivariateTestParameters>()));
            dal.Setup(d => d.Get(It.IsAny<Guid>())).Returns(new MultivariateTestParameters() { Id = testGuid });

            testManager.Archive(testGuid);

            dal.Verify(d => d.Update(It.Is<MultivariateTestParameters>(arg => arg.Id == testGuid && arg.State == "Archived")));
        }

        [TestMethod]
        public void Get_Returns_The_Test_With_The_Passed_In_Id()
        {
            var testManager = GetUnitUnderTest();
            var testGuid = Guid.NewGuid();

            dal.Setup(d => d.Get(It.Is<Guid>(a => a == testGuid))).Returns(new MultivariateTestParameters() { Id = testGuid });

            var actualTest = testManager.Get(testGuid);

            Assert.AreEqual(actualTest.Id, testGuid, "The Id returned should match the one supplied to the get method.");
        }

        [TestMethod]
        public void GetTestByItemId_Returns_The_Test_With_The_Given_Original_Id()
        {
            var testManager = GetUnitUnderTest();
            var itemGuid = Guid.NewGuid();

            dal.Setup(d => d.GetByOriginalItemId(It.IsAny<Guid>())).Returns(new MultivariateTestParameters[1] { new MultivariateTestParameters() { OriginalItemId = itemGuid } });

            var actualTest = testManager.GetTestByItemId(itemGuid);

            Assert.AreEqual(actualTest[0].OriginalItemId, itemGuid, "The test returned should have the same OriginalItemId as was requested");
        }

        [TestMethod]
        public void IncrementCount_Calls_Into_Dal_To_Update_The_Counts()
        {
            var testManager = GetUnitUnderTest();
            var testGuid = Guid.NewGuid();
            var itemGuid = Guid.NewGuid();

            testManager.IncrementCount(testGuid, itemGuid, CountType.View);
            testManager.IncrementCount(testGuid, itemGuid, CountType.Conversion);

            dal.Verify(d => d.UpdateViews(It.Is<Guid>(g => g == testGuid), It.Is<Guid>(g => g == itemGuid)), Times.Once, "Call to DAL should increment the tests given items views");
            dal.Verify(d => d.UpdateConversions(It.Is<Guid>(g => g == testGuid), It.Is<Guid>(g => g == itemGuid)), Times.Once, "Call to DAL should increment the tests given items conversions");
        }

        [TestMethod]
        public void ReturnLandingPage_Returns_OriginalOrVariantPage_BasedOnRandomNumberGenerator()
        {
            var testManager = GetUnitUnderTest();
            Guid testGuid = Guid.NewGuid(), origItemId = Guid.NewGuid(), varItemId = Guid.NewGuid();
            var mockTest = new MultivariateTestParameters()
            {
                Title = "Test 1",
                OriginalItemId = origItemId,
                VariantItemId = varItemId,

                // we only need one item in the list cause the code is kind of bogus right now and 
                // only returns the first item in the list anyway. We need a better test.
                VariantItems = new List<Guid>() { origItemId } 
            };

            dal.Setup(d => d.Get(It.IsAny<Guid>())).Returns(mockTest);

            var actualItemId = testManager.ReturnLandingPage(testGuid);
            Assert.IsTrue((actualItemId == mockTest.OriginalItemId || actualItemId == mockTest.VariantItemId), "The value returned was not equal to the expected Guid");
        }
    }
}
