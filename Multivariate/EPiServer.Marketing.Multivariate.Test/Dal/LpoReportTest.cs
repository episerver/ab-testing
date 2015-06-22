#region Copyright

// Copyright (c) 2010 EPiServer AB.  All rights reserved.
//   
// This code is released by EPiServer AB under the Source Code File - Specific License Conditions, published August 20, 2007. 
// See http://www.episerver.com/Specific_License_Conditions for details. 

#endregion

using System;
using System.Collections.Generic;
using EPiServer.Cmo.Core.Business;
using EPiServer.Cmo.Core.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPiServer.Cmo.DatabaseTests
{
    /// <summary>
    /// Summary description for LpoReportTest
    /// </summary>
    [TestClass]
    public class LpoReportTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        // Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void TestReportInitialize()
        {
            CMOTestHelper.InitHttpContext();
        }

        //OriginalConversion, OriginalView, Variation1Conversion, Variation1View, Variation2Conversion, Variation2View
        [DeploymentItem("EPiServer.Cmo.DatabaseTests\\LpoTestReportData.xml"), 
        DataSource("Microsoft.VisualStudio.TestTools.DataSource.XML", "|DataDirectory|\\LpoTestReportData.xml", "TestCase", DataAccessMethod.Sequential), 
        TestMethod]
        public void TestReport()
        {
            int i = TestContext.DataRow.Table.Rows.IndexOf(TestContext.DataRow);
            //using (CmoEntities dataContext = new CmoEntities())
            {
                List<LpoTest> tests = new List<LpoTest>();
                LpoTest test = LpoTest.CreateLpoTest("Test " + i, State.Paused, 100);
                test.StartDate = DateTime.Now;
                test.EndDate = DateTime.Now.AddDays(5);

                System.Diagnostics.Debug.WriteLine(TestContext.DataRow.Table.Columns.Count);
                for (int j = 0; j < TestContext.DataRow.Table.Columns.Count; j++)
                {
                    System.Diagnostics.Debug.WriteLine("Column #"+j+" "+TestContext.DataRow.Table.Columns[j].ColumnName + "=" + TestContext.DataRow[j]);
                }
                int originalView = Int32.Parse((String)TestContext.DataRow["OriginalView"]);
                int originalConversion = Int32.Parse((String)TestContext.DataRow["OriginalConversion"]);
                LpoTestPage pageOrg = LpoOriginalPage.CreateLpoOriginalPage(true, "Original " + i, 
                                                                            originalView,
                                                                            originalConversion, i + "1");
                test.LpoTestPages.Add(pageOrg);

                LpoTestPage pageCnv = LpoConversionPage.CreateLpoConversionPage(true, "Conversion " + i, 0, 0, i + "2");
                test.LpoTestPages.Add(pageCnv);
                LpoTestPage pageVar2 = null;
                if (TestContext.DataRow["Variation2View"] != DBNull.Value && TestContext.DataRow["Variation2Conversion"] != DBNull.Value && !String.IsNullOrEmpty(TestContext.DataRow["Variation2Conversion"] as String))
                {
                    int variation2View = Int32.Parse((String)TestContext.DataRow["Variation2View"]);
                    int variation2Conversion = Int32.Parse((String)TestContext.DataRow["Variation2Conversion"]);
                    pageVar2 = LpoVariationPage.CreateLpoVariationPage(true, "Var 2 " + i,
                                                                       variation2View,
                                                                       variation2Conversion, i + "3");
                    test.LpoTestPages.Add(pageVar2);
                }

                int variation1View = Int32.Parse((String)TestContext.DataRow["Variation1View"]);
                int variation1Conversion = Int32.Parse((String)TestContext.DataRow["Variation1Conversion"]);
                LpoTestPage pageVar1 = LpoVariationPage.CreateLpoVariationPage(true, "Var 1 " + i,
                                                                               variation1View,
                                                                               variation1Conversion, i + "4");
                test.LpoTestPages.Add(pageVar1);

                tests.Add(test);

                LpoTestPageResults resOriginal = LpoTestPageResults.GetTestPageResults(pageOrg);
                LpoTestPageResults.RemoveCachedTestPageResults(pageOrg); // cleaning cache, because LpoTestPage.ID is used as a part of a cache key,
                                                                         // but as we creating LpoTestPage in memory (without saving) all pages has ID equal to 0
                LpoTestPageResults resVar1 = LpoTestPageResults.GetTestPageResults(pageVar1);
                LpoTestPageResults.RemoveCachedTestPageResults(pageVar1); // cleaning cache 
                LpoTestPageResults resVar2 = LpoTestPageResults.GetTestPageResults(pageVar2);
                LpoTestPageResults.RemoveCachedTestPageResults(pageVar2); // cleaning cache 

                Assert.IsTrue(BothNullOrBothNotNull(resOriginal.ConversionRate, TestContext.DataRow["OriginalConversionsImpressions"]), "OriginalConversionsImpressions");
                Assert.IsTrue(BothNullOrBothNotNull(resOriginal.ChanceToBeatAll, TestContext.DataRow["OriginalChanceToBeatAll"]), "OriginalChanceToBeatAll");

                Assert.IsTrue(BothNullOrBothNotNull(resVar1.ConversionRate, TestContext.DataRow["Variation1ConversionsImpressions"]), "Variation1ConversionsImpressions");
                Assert.IsTrue(BothNullOrBothNotNull(resVar1.ChanceToBeatOriginal, TestContext.DataRow["Variation1ChanceToBeatOriginal"]), "Variation1ChanceToBeatOriginal");
                Assert.IsTrue(BothNullOrBothNotNull(resVar1.ChanceToBeatAll, TestContext.DataRow["Variation1ChanceToBeatAll"]), "Variation1ChanceToBeatAll");
                Assert.IsTrue(BothNullOrBothNotNull(resVar1.ObservedImprovement, TestContext.DataRow["Variation1ObserverdImprovement"]), "Variation1ObserverdImprovement");

                if (resVar2 !=null)
                {
                    Assert.IsTrue(BothNullOrBothNotNull(resVar2.ConversionRate, TestContext.DataRow["Variation2ConversionsImpressions"]), "Variation2ConversionsImpressions");
                    Assert.IsTrue(BothNullOrBothNotNull(resVar2.ChanceToBeatOriginal, TestContext.DataRow["Variation2ChanceToBeatOriginal"]), "Variation2ChanceToBeatOriginal");
                    Assert.IsTrue(BothNullOrBothNotNull(resVar2.ChanceToBeatAll, TestContext.DataRow["Variation2ChanceToBeatAll"]), "Variation2ChanceToBeatAll");
                    Assert.IsTrue(BothNullOrBothNotNull(resVar2.ObservedImprovement, TestContext.DataRow["Variation2ObserverdImprovement"]), "Variation2ObserverdImprovement");
                }

                //dataContext.AddToLpoTests(test);
                //dataContext.SaveChangesSkipSecurity();
            }
        }

        static bool BothNullOrBothNotNull(object value1, object value2)
        {
            if ((value1 == null || value1 == DBNull.Value || String.IsNullOrEmpty(value1 as String)) && (value2 == null || value2 == DBNull.Value || String.IsNullOrEmpty(value2 as String))) return true;
            if ((value1 != null && value1 != DBNull.Value || !String.IsNullOrEmpty(value1 as String)) && (value2 != null && value2 != DBNull.Value || !String.IsNullOrEmpty(value2 as String))) return true;
            return false;
        }
    }
}
