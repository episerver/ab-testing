using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using EPiServer.Enterprise;
using EPiServer.Marketing.Multivariate.Dal;
using EPiServer.Marketing.Multivariate.Model;
using EPiServer.Marketing.Multivariate.Model.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NMemory.Linq;
using TestContext = EPiServer.Marketing.Multivariate.Test.Dal.TestContext;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.Multivariate.Test.Core
{
    [TestClass]
    public class MultivariateManagerTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IMultiVariantDataAccess> _dataAccessLayer;

        private MultivariateTestManager GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _dataAccessLayer = new Mock<IMultiVariantDataAccess>();
            _serviceLocator.Setup(sl => sl.GetInstance<IMultiVariantDataAccess>()).Returns(_dataAccessLayer.Object);

            return new MultivariateTestManager(_serviceLocator.Object);
        }

        [TestMethod]
        public void MultivariateTestManager_CallsDataAccessGetWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.Get(theGuid);

            _dataAccessLayer.Verify(da => da.Get(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer get was never called or Guid did not match.");
        }
        [TestMethod]
        public void MultivariateTestManager_CallsDataAccessGetTestByItemId()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.GetTestByItemId(theGuid);

            _dataAccessLayer.Verify(da => da.GetTestByItemId(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer GetTestByItemId was never called or Guid did not match.");
        }

        [TestMethod]
        public void MultivariateTestManager_CallsGetTestListWithCritera()
        {
            var critera = new MultivariateTestCriteria();
            var tm = GetUnitUnderTest();
            tm.GetTestList(critera);

            _dataAccessLayer.Verify(da => da.GetTestList(It.Is<MultivariateTestCriteria>(arg => arg.Equals(critera))),
                "DataAcessLayer GetTestList was never called or criteria did not match.");
        }

        [TestMethod]
        public void MultivariateTestManager_CallsDeleteWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.Delete(theGuid);

            _dataAccessLayer.Verify(da => da.Delete(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer Delete was never called or Guid did not match.");
        }

        [TestMethod]
        public void MultivariateTestManager_CallsStartWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.Start(theGuid);

            _dataAccessLayer.Verify(da => da.Start(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer Start was never called or Guid did not match.");
        }

        [TestMethod]
        public void MultivariateTestManager_CallsStopWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.Stop(theGuid);

            _dataAccessLayer.Verify(da => da.Stop(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer Stop was never called or Guid did not match.");
        }
        [TestMethod]
        public void MultivariateTestManager_CallsArchiveWithGuid()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            tm.Archive(theGuid);

            _dataAccessLayer.Verify(da => da.Archive(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer Archive was never called or Guid did not match.");
        }
        [TestMethod]
        public void MultivariateTestManager_CallsSaveWithProperArguments()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var tm = GetUnitUnderTest();
            MultivariateTest test = new MultivariateTest() { Id = theGuid };
            tm.Save(test);

            _dataAccessLayer.Verify(da => da.Save(It.Is<MultivariateTest>(arg => arg.Equals(test))),
                "DataAcessLayer Save was never called or object did not match.");
        }
        [TestMethod]
        public void MultivariateTestManager_CallsIncrementCountWithProperArguments()
        {
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var theTestItemGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A4");
            CountType type = CountType.Conversion;

            var tm = GetUnitUnderTest();
            tm.IncrementCount(theGuid, theTestItemGuid, type);

            _dataAccessLayer.Verify(da => da.IncrementCount(It.Is<Guid>(arg => arg.Equals(theGuid)), It.IsAny<Guid>(), It.IsAny<CountType>()),
                "DataAcessLayer IncrementCount was never called or Test Guid did not match.");
            _dataAccessLayer.Verify(da => da.IncrementCount(It.IsAny<Guid>(), It.Is<Guid>(arg => arg.Equals(theTestItemGuid)), It.IsAny<CountType>()),
                "DataAcessLayer IncrementCount was never called or test item Guid did not match.");
            _dataAccessLayer.Verify(da => da.IncrementCount(It.IsAny<Guid>(), It.IsAny<Guid>(), It.Is<CountType>(arg => arg.Equals(CountType.Conversion))),
                "DataAcessLayer IncrementCount was never called or CountType did not match.");
        }

        [TestMethod]
        public void MultivariateTestManager_ReturnLandingPage_NoCache()
        {
            // Make sure that ther return landing page, calls data access layer if its not in the cache..
            var theGuid = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A3");
            var originalItemId = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A4");
            var vID = new Guid("A2AF4481-89AB-4D0A-B042-050FECEA60A5");
            var variantList = new List<Variant>() { new Variant { VariantId = vID } };

            var tm = GetUnitUnderTest();
            _dataAccessLayer.Setup(da => da.Get(It.Is<Guid>(arg => arg.Equals(theGuid)))).Returns(
                new MultivariateTest()
                {
                    Id = theGuid,
                    OriginalItemId = originalItemId,
                    Variants = variantList
                });

            // clear the cache if you have to (tm.clearCache() ?) - this test is supposed to verify that the 
            // database layer is called.
            Guid landingPage = tm.ReturnLandingPage(theGuid);

            _dataAccessLayer.Verify(da => da.Get(It.Is<Guid>(arg => arg.Equals(theGuid))),
                "DataAcessLayer get was never called or Guid did not match.");
            Assert.IsTrue(landingPage.Equals(originalItemId) ||
                landingPage.Equals(vID), "landingPage is not the original quid or the variant quid");
        }
    }
}