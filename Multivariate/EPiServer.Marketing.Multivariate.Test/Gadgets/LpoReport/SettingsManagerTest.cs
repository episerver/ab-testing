#region Copyright
// Copyright (c) 2010 EPiServer AB.  All rights reserved.
// 
// This code is released by EPiServer AB under the Source Code File - Specific License Conditions, published August 20, 2007. 
// See http://www.episerver.com/Specific_License_Conditions for details. 
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EPiServer.Cmo.Core.Entities;
using EPiServer.Cmo.Gadgets.Models.LpoReport;
using EPiServer.Cmo.Gadgets.Models.Shared;
using EPiServer.Framework.Localization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPiServer.Cmo.Gadgets.Tests.LpoReport
{
    [TestClass]
    public class SettingsManagerTest
    {
        private SettingsManager SettingsManager
        {
            get;
            set;
        }

        private Mock<ICmoDataHelper> CmoDataHelper
        {
            get;
            set;
        }

        /// <summary>
        ///   Gets or sets the fake campaign list to test Live Monitor.
        /// </summary>
        /// <value>The campaigns.</value>
        private List<LpoTest> LpoTests
        {
            get;
            set;
        }

        [TestInitialize]
        public void TestInitialize()
        {
            LpoTests = Utils.CreateLpoTests();
            CmoDataHelper = new Mock<ICmoDataHelper>();
            CmoDataHelper.Setup(helper => helper.GetLpoTestListWithPages()).Returns(LpoTests);
            SettingsManager = new SettingsManager(CmoDataHelper.Object, Utils.GetLocalizationServiceMock().Object);
        }

        #region Test Methods

        [TestMethod]
        public void LoadRelatedDataShouldFillTheLpoTestsProperty()
        {
            // Arrange
            Settings settings = new  Settings();

            // Act
            SettingsManager.LoadRelatedData(settings);

            // Assert
            CmoDataHelper.Verify(helper => helper.GetLpoTestListWithPages(), Times.Exactly(1), "Settings manager must call data helper method to get LpoTests list.");
            Assert.AreEqual(LpoTests.Count, settings.LpoTests.Count);
        }


        [TestMethod]
        public void IsConfuguredShouldBeTrueIfLpoTestIDIsSet()
        {
            // Arrange
            Settings settings = new Settings { LpoTestID = 1};

            // Act
            bool configured = SettingsManager.IsConfigured(settings);

            // Assert
            Assert.IsTrue(configured, "Settings are configured, when it contains LpoTestID > 0");
        }

        [TestMethod]
        public void IsConfuguredShouldBeFalseIfLpoTestIDIsNotSet()
        {
            // Arrange
            Settings settings = new Settings { LpoTestID = 0};

            // Act
            bool configured = SettingsManager.IsConfigured(settings);

            // Assert
            Assert.IsFalse(configured, "Settings are not configured, when it contains LpoTestID = 0.");
        }

        [TestMethod]
        public void IsValidShouldBeFalseIfCampaignIsNotFound()
        {
            // Arrange
            IEnumerable<ValidationError> errors;
            Settings settings = new Settings { LpoTestID = int.MaxValue};
            
            // Act
            bool isValid = SettingsManager.IsValid(settings, out errors);

            // Assert
            CmoDataHelper.Verify(helper => helper.GetLpoTestListWithPages(), Times.Exactly(1), "Settings manager must call data helper method.");
            Assert.IsFalse(isValid, "Settings must be processed as invalid when LpoTest is not found.");
            Assert.AreEqual(1, errors.Count(), "1 error must be returned.");
        }

        [TestMethod]
        public void IsValidShouldBeFalseIfLpoTestIdIsNotSet()
        {
            // Arrange
            IEnumerable<ValidationError> errors;
            Settings settings = new Settings { LpoTestID = 0 };

            // Act
            bool isValid = SettingsManager.IsValid(settings, out errors);

            // Assert
            Assert.IsFalse(isValid, "Settings are not valid, when it contains LpoTestID = 0.");
            Assert.AreEqual(1, errors.Count(), "1 error must be returned.");
        }

        #endregion

    }
}
