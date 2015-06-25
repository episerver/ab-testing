using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using log4net;
using Moq;
using EPiServer.Marketing.Multivariate.Dal;

namespace EPiServer.Marketing.Multivariate.Test.Core
{
    [TestClass]
    public class MultivariateManagerTests
    {
        private Mock<ILog> log;
        private Mock<IMultivariateTestDal> dal;
        private Mock<ICurrentUser> user;
        private Mock<IMultivariateTest> testData;
        private Mock<ICurrentSite> siteData;
        private string testUsername = "TestUser";
        private MultivariateTestManager GetUnitUnderTest()
        {
            log = new Mock<ILog>();
            dal = new Mock<IMultivariateTestDal>();
            user = new Mock<ICurrentUser>();
            siteData = new Mock<ICurrentSite>();
            testData = new Mock<IMultivariateTest>();
            
            user.Setup(u => u.GetDisplayName()).Returns(testUsername);
            return new MultivariateTestManager(log.Object, dal.Object, user.Object, siteData.Object);
        }

        [TestMethod]
        public void TestManager_Construction_Creates_Internal_Objects()
        {
            var aTestManager = new MultivariateTestManager();

            Assert.IsNotNull(aTestManager._log, "The logger should be created upon construction");
            Assert.IsNotNull(aTestManager._dataAccess, "The data access object should be created upon construction");
            Assert.IsNotNull(aTestManager._user, "The current user object should be created upon construction");
        }

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
        [ExpectedException(typeof(Exception))]
        public void Save_Throws_When_Original_Id_Is_In_Use()
        {
            var testManager = GetUnitUnderTest();
            testData.SetupGet(td => td.Id).Returns(null);
            testData.SetupGet(td => td.OriginalItemId).Returns(Guid.NewGuid());

            dal.Setup(d => d.GetByOriginalItemId(It.IsAny<Guid>())).Returns(new MultivariateTestParameters());

            var retId = testManager.Save(testData.Object);
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

            dal.Setup(d => d.GetByOriginalItemId(It.IsAny<Guid>())).Returns(new MultivariateTestParameters() { OriginalItemId = itemGuid });

            var actualTest = testManager.GetTestByItemId(itemGuid);

            Assert.AreEqual(actualTest.OriginalItemId, itemGuid, "The test returned should have the same OriginalItemId as was requested");
        }
    }
}
