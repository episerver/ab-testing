#region Copyright
// Copyright (c) 2010 EPiServer AB.  All rights reserved.
// 
// This code is released by EPiServer AB under the Source Code File - Specific License Conditions, published August 20, 2007. 
// See http://www.episerver.com/Specific_License_Conditions for details. 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPiServer.Cmo.Gadgets.Models.Shared;
using Moq;
using EPiServer.Cmo.Core.Entities;
using EPiServer.Cmo.Core.Business;
using EPiServer.Cmo.Gadgets.Models.LpoReport;
using EPiServer.Cmo.Core.Configuration;
using EPiServer.Cmo.Tests.MockTypes;
using System.Globalization;
using System.Threading;

namespace EPiServer.Cmo.Gadgets.Tests.LpoReport
{
    /// <summary>
    /// Summary description for DataManagerTest
    /// </summary>
    [TestClass]
    public class DataManagerTest
    {
        private CultureInfo Culture { get; set; }

        private DataManager DataManager { get; set; }

        private List<LpoTest> Tests { get; set; }

        #region Additional test attributes

        [TestInitialize]
        public void TestInitialize()
        {
            Culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            CmoSecurity._instance = new TestSecurity();
            CmoData._instance = new TestCmoData();
            CmoCache._instance = new CmoMemoryCache();
            CmoConfiguration.Default = new TestCmoConfiguration();

            Tests = Utils.CreateLpoTests();
            LanguageData languageData = new LanguageData {LanguageName = "English", LanguageIconUrl = ""};
            var mockCmoDataHelper = new Mock<ICmoDataHelper>();

            mockCmoDataHelper.Setup(helper => helper.GetLpoTestListWithPages()).Returns(Tests);
            mockCmoDataHelper.Setup(helper => helper.GetLanguageDataByID(It.IsAny<string>())).Returns(languageData);

            var results1 = Tests[0].LpoTestPages.Where(page => !(page is LpoConversionPage)).ToDictionary(page => page, page => LpoTestPageResults.GetTestPageResults(page));
            mockCmoDataHelper.Setup(helper => helper.GetLpoTestPagesWithResults(Tests[0])).Returns(results1);
            mockCmoDataHelper.Setup(helper => helper.FinalizeLpoTest(Tests[0], It.IsAny<int>())).Returns(true);
            mockCmoDataHelper.Setup(helper => helper.StartLpoTest(Tests[0])).Returns(true);
            mockCmoDataHelper.Setup(helper => helper.StopLpoTest(Tests[0])).Returns(true);
            mockCmoDataHelper.Setup(helper => helper.LpoTestIsValid(Tests[0])).Returns(true);

            var results2 = Tests[1].LpoTestPages.Where(page => !(page is LpoConversionPage)).ToDictionary(page => page, page => LpoTestPageResults.GetTestPageResults(page));
            mockCmoDataHelper.Setup(helper => helper.GetLpoTestPagesWithResults(Tests[1])).Returns(results2);
            mockCmoDataHelper.Setup(helper => helper.FinalizeLpoTest(Tests[1], It.IsAny<int>())).Returns(false);
            mockCmoDataHelper.Setup(helper => helper.StartLpoTest(Tests[1])).Returns(false);
            mockCmoDataHelper.Setup(helper => helper.StopLpoTest(Tests[1])).Returns(false);
            mockCmoDataHelper.Setup(helper => helper.LpoTestIsValid(Tests[1])).Returns(false);

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(helper => helper.GetCmoItemLink(It.IsAny<LpoTest>())).Returns("Test.aspx");
            mockUrlHelper.Setup(helper => helper.GetLpoTestPageThumbnailURL(It.IsAny<LpoTestPage>())).Returns("Thumbnail.aspx");
            mockUrlHelper.Setup(helper => helper.GetPageLink(It.IsAny<string>())).Returns("Page.aspx");

            DataManager = new DataManager(mockCmoDataHelper.Object, mockUrlHelper.Object, new DateHelper(), Utils.GetLocalizationServiceMock().Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Thread.CurrentThread.CurrentCulture = Culture;
        }
       
        #endregion

        #region GetData tests
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetDataShouldThrowErrorIfSettingsAreNull()
        {   
            DataManager.GetData(null);
        }

        [TestMethod]
        public void GetDataShouldReturnInvalidConfigurationIfSettingsAreNotConfigured()
        {
            Settings settings = new Settings { Configured = false };
            ViewData data = DataManager.GetData(settings);
            Assert.IsTrue(data.InvalidConfiguration);
        }

        [TestMethod]
        public void GetDataShouldReturnInvalidConfigurationIfTestNotFound()
        {
            Settings settings = new Settings { Configured = true, LpoTestID = 0 };
            ViewData data = DataManager.GetData(settings);
            Assert.IsTrue(data.InvalidConfiguration);
        }

        [TestMethod]
        public void GetDataForNormalTest()
        {
            Settings settings = new Settings { Configured = true, LpoTestID = 1 };
            ViewData data = DataManager.GetData(settings);
            IDateHelper dateHelper = new DateHelper();

            Assert.IsFalse(data.IsBroken);
            Assert.IsFalse(data.CanBeFinalized);
            Assert.IsFalse(data.CanBeStarted);
            Assert.IsTrue(data.CanBeStopped);
            Assert.IsTrue(data.ConversionPageExistsInCMS);
            Assert.AreEqual("Page.aspx", data.ConversionPageLink);
            Assert.AreEqual("ConversionPage1", data.ConversionPageName);
            Assert.IsFalse(data.InvalidConfiguration);
            Assert.IsTrue(data.IsEnoughData);
            Assert.AreEqual("English", data.Language.LanguageName);
            Assert.AreEqual("Test.aspx", data.Link);
            Assert.AreEqual("Test1", data.Name);
            Assert.AreEqual("Owner1", data.Owner);
            Assert.AreEqual(3, data.PagesData.Count());
            Assert.AreEqual(dateHelper.GetDateRangeAsString(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1)), data.Period);
            //Assert.AreEqual("Active", data.State); //localization-dependent assert
        }

        [TestMethod]
        public void GetDataForBrokenTest()
        {
            Settings settings = new Settings { Configured = true, LpoTestID = 2 };
            ViewData data = DataManager.GetData(settings);
            IDateHelper dateHelper = new DateHelper();

            //Assert.AreEqual(" Broken", data.Broken); //localization-dependent assert
            Assert.IsFalse(data.CanBeFinalized);
            Assert.IsFalse(data.CanBeStarted);
            Assert.IsFalse(data.CanBeStopped);
            Assert.IsFalse(data.ConversionPageExistsInCMS);
            Assert.AreEqual("Page.aspx", data.ConversionPageLink);
            Assert.AreEqual("ConversionPage2", data.ConversionPageName);
            Assert.IsFalse(data.InvalidConfiguration);
            Assert.IsFalse(data.IsEnoughData);
            Assert.AreEqual("English", data.Language.LanguageName);
            Assert.AreEqual("Test.aspx", data.Link);
            Assert.AreEqual("Test2", data.Name);
            Assert.AreEqual("Owner2", data.Owner);
            Assert.AreEqual(2, data.PagesData.Count());
            Assert.AreEqual(dateHelper.GetDateRangeAsString(DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-1)), data.Period);
            //Assert.AreEqual("Finalized", data.State); //localization-dependent assert
        }

        #endregion

        #region GetTestPageData tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetTestPageDataShouldThrowErrorIfPageIsNull()
        {
            DataManager.GetTestPageData(null, LpoTestPageResults.GetTestPageResults(Tests[0].OriginalPage));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetTestPageDataShouldThrowErrorIfResultsAreNull()
        {
            DataManager.GetTestPageData(Tests[0].OriginalPage, null);
        }

        [TestMethod]
        public void GetTestPageDataForOriginalPage()
        {
            TestPageData data = DataManager.GetTestPageData(Tests[0].OriginalPage, LpoTestPageResults.GetTestPageResults(Tests[0].OriginalPage));

            Assert.AreEqual(string.Empty, data.BarCssClass);
            Assert.AreEqual(0.261, Math.Round(data.ChanceToBeatAll, 3));
            Assert.AreEqual("26.1 %", data.ChanceToBeatAllString);
            Assert.AreEqual(0.0, data.ChanceToBeatOriginal);
            //Assert.AreEqual("--", data.ChanceToBeatOriginalString); //localization-dependent assert
            Assert.AreEqual(0, data.ComparePageResult);
            Assert.AreEqual(0.928, Math.Round(data.ConversionRate, 3));
            Assert.AreEqual("92.8", data.ConversionRateString);
            Assert.AreEqual(0.032, Math.Round(data.ConversionRateRange, 3));
            Assert.AreEqual("± 3.2 %", data.ConversionRateRangeString);
            Assert.AreEqual("92.8 ± 3.2 %", data.ConversionRateWithRangeString);
            Assert.AreEqual(103, data.Conversions);
            Assert.AreEqual(string.Empty, data.CssClass);
            Assert.AreEqual(1, data.ID);
            Assert.AreEqual(111, data.Impressions);
            Assert.IsTrue(data.IsOriginal);
            Assert.IsFalse(data.IsWinner);
            Assert.AreEqual(0.0, data.ObservedImprovement);
            //Assert.AreEqual("--", data.ObservedImprovementString); //localization-dependent assert
            Assert.AreEqual(0.928, Math.Round(data.OriginalPageConversionRate, 3));
            Assert.AreEqual(0.032, Math.Round(data.OriginalPageConversionRateRange, 3));
            Assert.IsTrue(data.PageExistsInCMS);
            Assert.AreEqual("Page.aspx", data.PageLink);
            Assert.AreEqual("OriginalPage1", data.PageName);
            Assert.AreEqual("Thumbnail.aspx", data.PageThumbnailUrl);
            //Assert.AreEqual("Original", data.TypeString); //localization-dependent assert
        }

        [TestMethod]
        public void GetTestPageDataForVariationPage()
        {
            TestPageData data = DataManager.GetTestPageData(Tests[0].VariationPage1, LpoTestPageResults.GetTestPageResults(Tests[0].VariationPage1));

            Assert.AreEqual("epi-LPOGadget-bar-positive", data.BarCssClass);
            Assert.AreEqual(0.631, Math.Round(data.ChanceToBeatAll, 3));
            Assert.AreEqual("63.1 %", data.ChanceToBeatAllString);
            Assert.AreEqual(0.705, Math.Round(data.ChanceToBeatOriginal, 3));
            Assert.AreEqual("70.5 %", data.ChanceToBeatOriginalString);
            Assert.AreEqual(0.945, Math.Round(data.ConversionRate, 3));
            Assert.AreEqual(1, data.ComparePageResult);
            Assert.AreEqual("94.5", data.ConversionRateString);
            Assert.AreEqual(0.028, Math.Round(data.ConversionRateRange, 3));
            Assert.AreEqual("± 2.8 %", data.ConversionRateRangeString);
            Assert.AreEqual("94.5 ± 2.8 %", data.ConversionRateWithRangeString);
            Assert.AreEqual(104, data.Conversions);
            Assert.AreEqual("epi-color-green", data.CssClass);
            Assert.AreEqual(2, data.ID);
            Assert.AreEqual(110, data.Impressions);
            Assert.IsFalse(data.IsOriginal);
            Assert.IsFalse(data.IsWinner);
            Assert.AreEqual(0.019, Math.Round(data.ObservedImprovement, 3));
            Assert.AreEqual("+1.9 %", data.ObservedImprovementString);
            Assert.AreEqual(0.928, Math.Round(data.OriginalPageConversionRate, 3));
            Assert.AreEqual(0.032, Math.Round(data.OriginalPageConversionRateRange, 3));
            Assert.IsFalse(data.PageExistsInCMS);
            Assert.AreEqual("Page.aspx", data.PageLink);
            Assert.AreEqual("VariationPage11", data.PageName);
            Assert.AreEqual("Thumbnail.aspx", data.PageThumbnailUrl);
            //Assert.AreEqual("Variation 1", data.TypeString); //localization-dependent assert
        }

        [TestMethod]
        public void GetTestPageDataForNotEnoughData()
        {
            TestPageData data = DataManager.GetTestPageData(Tests[1].VariationPage1, LpoTestPageResults.GetTestPageResults(Tests[1].VariationPage1));

            Assert.AreEqual("epi-LPOGadget-bar-negative", data.BarCssClass);
            Assert.AreEqual(0.0, data.ChanceToBeatAll);
            //Assert.AreEqual("--", data.ChanceToBeatAllString); //localization-dependent assert
            Assert.AreEqual(0.0, data.ChanceToBeatOriginal);
            //Assert.AreEqual("--", data.ChanceToBeatOriginalString); //localization-dependent assert
            Assert.AreEqual(-1, data.ComparePageResult);
            Assert.AreEqual(0.897, Math.Round(data.ConversionRate, 3));
            Assert.AreEqual("89.7", data.ConversionRateString);
            Assert.AreEqual(0.040, Math.Round(data.ConversionRateRange, 3));
            Assert.AreEqual("± 4.0 %", data.ConversionRateRangeString);
            Assert.AreEqual("89.7 ± 4.0 %", data.ConversionRateWithRangeString);
            Assert.AreEqual(87, data.Conversions);
            Assert.AreEqual("epi-color-red", data.CssClass);
            Assert.AreEqual(6, data.ID);
            Assert.AreEqual(97, data.Impressions);
            Assert.IsFalse(data.IsOriginal);
            Assert.IsFalse(data.IsWinner);
            Assert.AreEqual(0.0, data.ObservedImprovement);
            //Assert.AreEqual("--", data.ObservedImprovementString); //localization-dependent assert
            Assert.AreEqual(0.936, Math.Round(data.OriginalPageConversionRate, 3));
            Assert.AreEqual(0.030, Math.Round(data.OriginalPageConversionRateRange, 3));
            Assert.IsTrue(data.PageExistsInCMS);
            Assert.AreEqual("Page.aspx", data.PageLink);
            Assert.AreEqual("VariationPage21", data.PageName);
            Assert.AreEqual("Thumbnail.aspx", data.PageThumbnailUrl);
            //Assert.AreEqual("Variation 1", data.TypeString); //localization-dependent assert
        }

        #endregion

        #region StartTest tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StartTestShouldThrowErrorIfSettingsAreNull()
        {
            DataManager.StartTest(null);
        }

        [TestMethod]
        public void StartTestShouldReturnFalseIfSettingsAreNotConfigured()
        {
            Settings settings = new Settings { Configured = false };
            var result = DataManager.StartTest(settings);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(CmoException))]
        public void StartTestShouldThrowErrorIfTestNotFound()
        {
            Settings settings = new Settings { Configured = true, LpoTestID = 0 };
            DataManager.StartTest(settings);
        }

        [TestMethod]
        public void StartTestForNormalTest()
        {
            Settings settings = new Settings { Configured = true, LpoTestID = 1 };
            var result = DataManager.StartTest(settings);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void StartTestForBrokenTest()
        {
            Settings settings = new Settings { Configured = true, LpoTestID = 2 };
            var result = DataManager.StartTest(settings);
            Assert.IsFalse(result);
        }

        #endregion

        #region StopTest tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void StopTestShouldThrowErrorIfSettingsAreNull()
        {
            DataManager.StopTest(null);
        }

        [TestMethod]
        public void StopTestShouldReturnFalseIfSettingsAreNotConfigured()
        {
            Settings settings = new Settings { Configured = false };
            var result = DataManager.StopTest(settings);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(CmoException))]
        public void StopTestShouldThrowErrorIfTestNotFound()
        {
            Settings settings = new Settings { Configured = true, LpoTestID = 0 };
            DataManager.StopTest(settings);
        }

        [TestMethod]
        public void StopTestForNormalTest()
        {
            Settings settings = new Settings { Configured = true, LpoTestID = 1 };
            var result = DataManager.StopTest(settings);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void StopTestForBrokenTest()
        {
            Settings settings = new Settings { Configured = true, LpoTestID = 2 };
            var result = DataManager.StopTest(settings);
            Assert.IsFalse(result);
        }

        #endregion

        #region FinalizeTest tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FinalizeTestShouldThrowErrorIfSettingsAreNull()
        {
            DataManager.FinalizeTest(null, 1);
        }

        [TestMethod]
        public void FinalizeTestShouldReturnFalseIfSettingsAreNotConfigured()
        {
            Settings settings = new Settings { Configured = false };
            var result = DataManager.FinalizeTest(settings, 1);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(CmoException))]
        public void FinalizeTestShouldThrowErrorIfTestNotFound()
        {
            Settings settings = new Settings { Configured = true, LpoTestID = 0 };
            DataManager.FinalizeTest(settings, 1);
        }

        [TestMethod]
        public void FinalizeTestForNormalTest()
        {
            Settings settings = new Settings { Configured = true, LpoTestID = 1 };
            var result = DataManager.FinalizeTest(settings, 1);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void FinalizeTestForBrokenTest()
        {
            Settings settings = new Settings { Configured = true, LpoTestID = 2 };
            var result = DataManager.FinalizeTest(settings, 6);
            Assert.IsFalse(result);
        }

        #endregion
    }
}
