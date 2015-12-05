#region Copyright
// Copyright (c) 2010 EPiServer AB.  All rights reserved.
// 
// This code is released by EPiServer AB under the Source Code File - Specific License Conditions, published August 20, 2007. 
// See http://www.episerver.com/Specific_License_Conditions for details. 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using EPiServer.Cmo.Gadgets.Controllers;
using EPiServer.Cmo.Gadgets.Models.LpoReport;
using EPiServer.Cmo.Gadgets.Models.Shared;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPiServer.Cmo.Gadgets.Tests.LpoReport
{
    [TestClass]
    public class LpoReportControllerTests
    {
        /// <summary>
        ///   Settings manager mock.
        /// </summary>
        /// <value>The settings manager.</value>
        private Mock<ISettingsManager> SettingsManager
        {
            get;
            set;
        }

        /// <summary>
        ///   Gets or sets the data manager mock.
        /// </summary>
        /// <value>The data manager mock.</value>
        private Mock<IDataManager> DataManager
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the context helper.
        /// </summary>
        /// <value>The context helper.</value>
        private Mock<IContextHelper> ContextHelper
        {
            get;
            set;
        }

        /// <summary>
        ///   Gets or sets the controller.
        /// </summary>
        /// <value>The controller.</value>
        private LpoReportController Controller
        {
            get;
            set;
        }

        /// <summary>
        ///   Gets or sets the gadget settings.
        /// </summary>
        /// <value>The settings.</value>
        private Settings Settings
        {
            get;
            set;
        }

        /// <summary>
        ///   Gets or sets the gadget ID.
        /// </summary>
        /// <value>The gadget ID.</value>
        private Guid GadgetID
        {
            get;
            set;
        }

        /// <summary>
        ///   Initializes test.
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            SettingsManager = new Mock<ISettingsManager>();
            DataManager = new Mock<IDataManager>();
            ContextHelper = new Mock<IContextHelper>();
            Controller = new LpoReportController(SettingsManager.Object, DataManager.Object, ContextHelper.Object)
                             {
                                 ControllerContext = Utils.GetMockControllerContext().Object
                             };
            Settings = new Settings();
            GadgetID = Guid.NewGuid();
        }

        [TestMethod]
        public void IndexActionShouldReturnNotConfiguredViewIfSettingsNotConfigured()
        {
            // Arrange
            Settings notConfiguredSettings = new Settings {Configured = false};
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(notConfiguredSettings);
            IEnumerable<ValidationError> errors;
            SettingsManager.Setup(manager => manager.IsValid(notConfiguredSettings, out errors)).Returns(true);
            DataManager.Setup(manager => manager.GetData(notConfiguredSettings)).Returns(new ViewData());

            // Act
            ViewResult viewResult = (ViewResult) Controller.Index(GadgetID);

            // Assert
            Assert.AreEqual("NotConfigured", viewResult.ViewName);
        }

        [TestMethod]
        public void IndexActionShouldReturnNotConfiguredViewIfSettingsNotValid()
        {
            // Arrange
            Settings configuredSettings = new Settings { Configured = true };
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(configuredSettings);
            IEnumerable<ValidationError> errors = new List<ValidationError>();
            SettingsManager.Setup(manager => manager.IsValid(configuredSettings, out errors))
                .Callback(() => ((List<ValidationError>)errors).Add(new ValidationError { ErrorMessage = "TestError", PropertyName = "LpoTestID" }))
                .Returns(false);
            DataManager.Setup(manager => manager.GetData(configuredSettings)).Returns(new ViewData());

            // Act
            ViewResult viewResult = (ViewResult)Controller.Index(GadgetID);

            // Assert
            Assert.AreEqual("NotConfigured", viewResult.ViewName);
        }


        [TestMethod]
        public void IndexActionShouldReturnNotConfiguredViewIfViewDataInvalidConfigurationIsTrue()
        {
            // Arrange
            Settings configuredSettings = new Settings { Configured = true };
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(configuredSettings);
            IEnumerable<ValidationError> errors;
            SettingsManager.Setup(manager => manager.IsValid(configuredSettings, out errors)).Returns(true);
            DataManager.Setup(manager => manager.GetData(configuredSettings)).Returns(new ViewData {InvalidConfiguration = true});

            // Act
            ViewResult viewResult = (ViewResult)Controller.Index(GadgetID);

            // Assert
            Assert.AreEqual("NotConfigured", viewResult.ViewName);
        }

        [TestMethod]
        public void IndexActionShouldReturnIndexView()
        {
            // Arrange
            Settings configuredSettings = new Settings { Configured = true };
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(configuredSettings);
            IEnumerable<ValidationError> errors;
            SettingsManager.Setup(manager => manager.IsValid(configuredSettings, out errors)).Returns(true);
            DataManager.Setup(manager => manager.GetData(configuredSettings)).Returns(new ViewData());
            ContextHelper.Setup(helper => helper.CheckIsAppleDevice(It.IsAny<HttpRequestBase>())).Returns(false);

            // Act
            ViewResult viewResult = (ViewResult)Controller.Index(GadgetID);

            // Assert
            Assert.AreEqual("Index", viewResult.ViewName);
        }

        [TestMethod]
        public void IndexActionShouldReturnIndexCompactViewIfCompactMode()
        {
            // Arrange
            Settings compactViewSettings = new Settings {Configured = true, IsCompactView = true};
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(compactViewSettings);
            IEnumerable<ValidationError> errors;
            SettingsManager.Setup(manager => manager.IsValid(compactViewSettings, out errors)).Returns(true);
            DataManager.Setup(manager => manager.GetData(compactViewSettings)).Returns(new ViewData());
            ContextHelper.Setup(helper => helper.CheckIsAppleDevice(It.IsAny<HttpRequestBase>())).Returns(false);

            // Act
            ViewResult viewResult = (ViewResult)Controller.Index(GadgetID);

            // Assert
            Assert.AreEqual("IndexCompact", viewResult.ViewName);
        }

        [TestMethod]
        public void IndexActionShouldReturnIndexMobileView()
        {
            // Arrange
            Settings configuredSettings = new Settings { Configured = true };
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(configuredSettings);
            IEnumerable<ValidationError> errors;
            SettingsManager.Setup(manager => manager.IsValid(configuredSettings, out errors)).Returns(true);
            DataManager.Setup(manager => manager.GetData(configuredSettings)).Returns(new ViewData());
            ContextHelper.Setup(helper => helper.CheckIsAppleDevice(It.IsAny<HttpRequestBase>())).Returns(true);

            // Act
            ViewResult viewResult = (ViewResult)Controller.Index(GadgetID);

            // Assert
            Assert.AreEqual("IndexMobile", viewResult.ViewName);
        }

        [TestMethod]
        public void IndexActionShouldReturnIndexMobileCompactViewIfCompactMode()
        {
            // Arrange
            Settings compactViewSettings = new Settings { Configured = true, IsCompactView = true };
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(compactViewSettings);
            IEnumerable<ValidationError> errors;
            SettingsManager.Setup(manager => manager.IsValid(compactViewSettings, out errors)).Returns(true);
            DataManager.Setup(manager => manager.GetData(compactViewSettings)).Returns(new ViewData());
            ContextHelper.Setup(helper => helper.CheckIsAppleDevice(It.IsAny<HttpRequestBase>())).Returns(true);

            // Act
            ViewResult viewResult = (ViewResult)Controller.Index(GadgetID);

            // Assert
            Assert.AreEqual("IndexMobileCompact", viewResult.ViewName);
        }



        [TestMethod]
        public void EditSettingsGetShouldReturnEditSettingsView()
        {
            // Arrange
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(Settings);

            // Act
            ViewResult viewResult = (ViewResult) Controller.EditSettings(GadgetID);

            // Assert
            SettingsManager.Verify(manager=>manager.Load(GadgetID));
            Assert.AreEqual("EditSettings", viewResult.ViewName);
            Assert.AreEqual(Settings, viewResult.ViewData.Model);

        }

        [TestMethod]
        public void EditSettingsPostShouldReturnEditSettingsViewIfSettingsInvalid()
        {
            // Arrange
            IEnumerable<ValidationError> errors = new List<ValidationError>();
            SettingsManager.Setup(manager => manager.IsValid(Settings, out errors))
                .Callback(()=>((List<ValidationError>)errors).Add(new ValidationError {ErrorMessage = "TestError", PropertyName = "LpoTestID"}))
                .Returns(false);

            // Act
            ViewResult viewResult = (ViewResult) Controller.EditSettings(GadgetID, Settings);

            // Assert
            SettingsManager.Verify(manager=>manager.Save(It.IsAny<Settings>()), Times.Never());
            Assert.AreEqual("EditSettings",viewResult.ViewName);
            Assert.IsFalse(viewResult.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void EditSettingsPostShouldSaveValidSettings()
        {
            // Arrange
            IEnumerable<ValidationError> errors = new List<ValidationError>();
            SettingsManager.Setup(manager => manager.IsValid(Settings, out errors)).Returns(true);
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(Settings);

            // Act
            ViewResult viewResult = (ViewResult)Controller.EditSettings(GadgetID, Settings);

            // Assert
            SettingsManager.Verify(manager=>manager.Save(Settings));
        }

        [TestMethod]
        public void StartSucceededMustReturnNoErrors()
        {
            IEnumerable<ValidationError> errors = new List<ValidationError>();
            SettingsManager.Setup(manager => manager.IsValid(Settings, out errors)).Returns(true);
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(Settings);
            DataManager.Setup(manager => manager.StartTest(Settings)).Returns(true);
            
            ViewResult viewResult = (ViewResult)Controller.Start(GadgetID);

            Assert.IsTrue(viewResult.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void StartFailedMustReturnErrors()
        {
            IEnumerable<ValidationError> errors = new List<ValidationError>();
            SettingsManager.Setup(manager => manager.IsValid(Settings, out errors)).Returns(true);
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(Settings);
            DataManager.Setup(manager => manager.StartTest(Settings)).Returns(false);

            ViewResult viewResult = (ViewResult)Controller.Start(GadgetID);

            Assert.IsFalse(viewResult.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void StopSucceededMustReturnNoErrors()
        {
            IEnumerable<ValidationError> errors = new List<ValidationError>();
            SettingsManager.Setup(manager => manager.IsValid(Settings, out errors)).Returns(true);
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(Settings);
            DataManager.Setup(manager => manager.StopTest(Settings)).Returns(true);

            ViewResult viewResult = (ViewResult)Controller.Stop(GadgetID);

            Assert.IsTrue(viewResult.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void StopFailedMustReturnErrors()
        {
            IEnumerable<ValidationError> errors = new List<ValidationError>();
            SettingsManager.Setup(manager => manager.IsValid(Settings, out errors)).Returns(true);
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(Settings);
            DataManager.Setup(manager => manager.StopTest(Settings)).Returns(false);

            ViewResult viewResult = (ViewResult)Controller.Stop(GadgetID);

            Assert.IsFalse(viewResult.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void FinalizeSucceededMustReturnNoErrors()
        {
            IEnumerable<ValidationError> errors = new List<ValidationError>();
            SettingsManager.Setup(manager => manager.IsValid(Settings, out errors)).Returns(true);
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(Settings);
            DataManager.Setup(manager => manager.FinalizeTest(Settings, It.IsAny<Int32>())).Returns(true);

            ViewResult viewResult = (ViewResult)Controller.Finalize(GadgetID, 0);

            Assert.IsTrue(viewResult.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void FinalizeFailedMustReturnErrors()
        {
            IEnumerable<ValidationError> errors = new List<ValidationError>();
            SettingsManager.Setup(manager => manager.IsValid(Settings, out errors)).Returns(true);
            SettingsManager.Setup(manager => manager.Load(GadgetID)).Returns(Settings);
            DataManager.Setup(manager => manager.FinalizeTest(Settings, It.IsAny<Int32>())).Returns(false);

            ViewResult viewResult = (ViewResult)Controller.Finalize(GadgetID, 0);

            Assert.IsFalse(viewResult.ViewData.ModelState.IsValid);
        }
    }
}
