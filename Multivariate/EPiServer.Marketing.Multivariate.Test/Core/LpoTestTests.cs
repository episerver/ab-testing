#region Copyright
// Copyright (c) 2010 EPiServer AB.  All rights reserved.
// 
// This code is released by EPiServer AB under the Source Code File - Specific License Conditions, published August 20, 2007. 
// See http://www.episerver.com/Specific_License_Conditions for details. 
#endregion

using EPiServer.Cmo.Core.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPiServer.Cmo.Tests
{
    [TestClass]
    public class LpoTestTests
    {
        [TestMethod]
        public void SetOriginalPageShouldUpdateTestsOriginalPage()
        {
            // Arrange
            LpoTest test = LpoTest.CreateLpoTest("Test", State.Run, 100);
            LpoOriginalPage originalPage = LpoOriginalPage.CreateLpoOriginalPage(true, "OriginalPage", 0, 0, "3");
            test.LpoTestPages.Add(originalPage);

            // Act
            test.SetOriginalPage("4", "NewOriginalPage");

            // Assert
            Assert.AreEqual("4", test.OriginalPage.Reference);
            Assert.AreEqual("NewOriginalPage", test.OriginalPage.Name);

        }

        [TestMethod]
        public void SetConversionPageShouldUpdateTestsConversionPage()
        {
            // Arrange
            LpoTest test = LpoTest.CreateLpoTest("Test", State.Run, 100);
            LpoConversionPage conversionPage = LpoConversionPage.CreateLpoConversionPage(true, "ConversionPage", 0, 0, "3");
            test.LpoTestPages.Add(conversionPage);

            // Act
            test.SetConversionPage("4", "NewConversionPage");

            // Assert
            Assert.AreEqual("4", test.ConversionPage.Reference);
            Assert.AreEqual("NewConversionPage", test.ConversionPage.Name);

        }

        [TestMethod]
        public void SetVariation1PageShouldUpdateTestsVariation1Page()
        {
            // Arrange
            LpoTest test = LpoTest.CreateLpoTest("Test", State.Run, 100);
            LpoVariationPage variationPage = LpoVariationPage.CreateLpoVariationPage(true, "Variation1Page", 0, 0, "3");
            test.LpoTestPages.Add(variationPage);

            // Act
            test.SetVariation1Page("4", "NewVariation1Page");

            // Assert
            Assert.AreEqual("4", test.VariationPage1.Reference);
            Assert.AreEqual("NewVariation1Page", test.VariationPage1.Name);

        }

        [TestMethod]
        public void SetVariation2PageShouldUpdateTestsVariation2Page()
        {
            // Arrange
            LpoTest test = LpoTest.CreateLpoTest("Test", State.Run, 100);
            LpoVariationPage variation1Page = LpoVariationPage.CreateLpoVariationPage(true, "Variation1Page", 0, 0, "3");
            LpoVariationPage variation2Page = LpoVariationPage.CreateLpoVariationPage(true, "Variation2Page", 0, 0, "5");
            test.LpoTestPages.Add(variation1Page);
            test.LpoTestPages.Add(variation2Page);

            // Act
            test.SetVariation2Page("4", "NewVariation2Page");

            // Assert
            Assert.AreEqual("4", test.VariationPage2.Reference);
            Assert.AreEqual("NewVariation2Page", test.VariationPage2.Name);

        }
    }
}
