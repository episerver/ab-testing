#region Copyright
// Copyright (c) 2010 EPiServer AB.  All rights reserved.
// 
// This code is released by EPiServer AB under the Source Code File - Specific License Conditions, published August 20, 2007. 
// See http://www.episerver.com/Specific_License_Conditions for details. 
#endregion

using System;
using System.Data.Objects;
using System.Linq;
using System.Transactions;
using EPiServer.Cmo.Core.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPiServer.Cmo.DatabaseTests
{
    [TestClass]
    public class LpoTestTests
    {
        private TransactionScope _transactionScope;

        ///<summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void ConversionPathTestInitialize()
        {
            CMOTestHelper.InitHttpContext();
            _transactionScope = new TransactionScope();
        }

        //
        // Use TestCleanup to run code after each test has run
        [TestCleanup]
        public void MyTestCleanup()
        {
            _transactionScope.Dispose();
        }

        #endregion

        [TestMethod]
        public void ClearPageDataShouldDeleteAllTestsPageData()
        {
            // Arrange 
            int beforeCount = 0;
            LpoTest lpoTest;
            using (CmoEntities context = new CmoEntities())
            {

                lpoTest = LpoTest.CreateLpoTest("Test", State.NotActive, 100);
                context.AddToLpoTests(lpoTest);

                LpoOriginalPage originalPage = LpoOriginalPage.CreateLpoOriginalPage(true, "originalPage", 0, 0, "3");
                lpoTest.LpoTestPages.Add(originalPage);
                originalPage.LpoTestPageData = new LpoTestPageData {  Preview = new byte[3] { 1, 2, 3 } };

                LpoConversionPage conversionPage = LpoConversionPage.CreateLpoConversionPage(true, "conversionPage", 0, 0, "4");
                lpoTest.LpoTestPages.Add(conversionPage);
                conversionPage.LpoTestPageData = new LpoTestPageData { Preview = new byte[2] { 4, 5 } };

                context.SaveChangesSkipSecurity();
                var pageDataBefore = context.LpoTests.First(c => c.Name == "Test").LpoTestPages.Select(p => p.LpoTestPageData).ToList();
                beforeCount = pageDataBefore.Count;
            }

            // Act
            lpoTest.ClearPageData();

            // Assert
            using (CmoEntities context = new CmoEntities())
            {
                Assert.AreEqual(2, beforeCount);
                var pageDataAfter = context.LpoTests.First(c => c.Name == "Test").LpoTestPages.Select(p => p.LpoTestPageData).ToList();
                Assert.AreEqual(0, pageDataAfter.Count);
            }
        }

        [TestMethod]
        public void DeleteDetachedShouldDeleteTest()
        {
            // Arrange
            LpoTest lpoTest;
            using (CmoEntities context = new CmoEntities())
            {

                lpoTest = LpoTest.CreateLpoTest("Test", State.NotActive, 100);
                context.AddToLpoTests(lpoTest);
                context.SaveChangesSkipSecurity();
            }

            // Act
            lpoTest.DeleteDetached();

            // Assert
            using (CmoEntities context = new CmoEntities())
            {
                LpoTest loadedTest = context.LpoTests.FirstOrDefault(c => c.Name == "Test");
                Assert.IsNull(loadedTest);
            }
        }

        [TestMethod]
        public void UpdateDetachedShouldUpdateTest()
        {
            // Arrange
            LpoTest lpoTest;
            using (CmoEntities context = new CmoEntities())
            {
                lpoTest = LpoTest.CreateLpoTest("Test", State.NotActive, 100);
                context.AddToLpoTests(lpoTest);
                context.SaveChangesSkipSecurity();
            }

            // Act
            lpoTest.Name = "NewTestName";
            lpoTest.UpdateDetached();

            // Assert
            using (CmoEntities context = new CmoEntities())
            {
                LpoTest loadedTest = context.LpoTests.FirstOrDefault(c => c.Name == "NewTestName");
                Assert.IsNotNull(loadedTest);
            }
        }

    }
}
