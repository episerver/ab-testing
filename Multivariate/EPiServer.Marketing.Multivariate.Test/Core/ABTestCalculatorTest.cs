#region Copyright

// Copyright (c) 2010 EPiServer AB.  All rights reserved.
//   
// This code is released by EPiServer AB under the Source Code File - Specific License Conditions, published August 20, 2007. 
// See http://www.episerver.com/Specific_License_Conditions for details. 

#endregion

using System;
using EPiServer.Cmo.Core.Business;
using EPiServer.Cmo.Core.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPiServer.Cmo.Tests
{
    /// <summary>
    /// Tests for ABTestCalculator class
    /// </summary>
    [TestClass]
    public class ABTestCalculatorTest
    {
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

        private static void Compare(double value1, double value2)
        {
            value1 = Math.Round(value1, 5);
            value2 = Math.Round(value2, 5);
            Assert.AreEqual(value1, value2);
        }

        [TestMethod]
        public void TestCalculateConversionRate()
        {
            Compare(0.31172, ABTestCalculator.CalculateConversionRate(401, 125));
            Compare(0.38929, ABTestCalculator.CalculateConversionRate(411, 160));
            Compare(0.55556, ABTestCalculator.CalculateConversionRate(999, 555));
        }

        [TestMethod]
        public void TestCalculateObservedImprovement()
        {
            Compare(0.24886, ABTestCalculator.CalculateObservedImprovement(401, 125, 411, 160));
            Compare(0.78222, ABTestCalculator.CalculateObservedImprovement(401, 125, 999, 555));
        }

        [TestMethod]
        public void TestCalculateConversionRateRange()
        {
            Compare(0.02969, ABTestCalculator.CalculateConversionRateRange(401, 125));
            Compare(0.03087, ABTestCalculator.CalculateConversionRateRange(411, 160));
            Compare(0.02016, ABTestCalculator.CalculateConversionRateRange(999, 555));
        }

        [TestMethod]
        public void TestCalculateChanceToBeatOriginal()
        {
            Compare(0.98983, ABTestCalculator.CalculateChanceToBeatOriginal(401, 125, 411, 160));
            Compare(1.00000, ABTestCalculator.CalculateChanceToBeatOriginal(401, 125, 999, 555));
        }



        [TestMethod]
        [ExpectedException(typeof(CmoABTestException))]
        public void TestCalculateChanceToBeatAll()
        {
            LpoTestPage page1 = LpoVariationPage.CreateLpoVariationPage(true, "p1", 3273, 951, "1_1");
            LpoTestPage page2 = LpoVariationPage.CreateLpoVariationPage(true, "p2", 3380, 1099, "2_1");
            LpoTestPage page3 = LpoVariationPage.CreateLpoVariationPage(true, "p3", 3347, 975, "3_1");

            ChanceToBeatAllCalculator calculator1 = new ChanceToBeatAllCalculator();
            ChanceToBeatAllCalculator calculator2 = new ChanceToBeatAllCalculator();
            ChanceToBeatAllCalculator calculator3 = new ChanceToBeatAllCalculator();

            calculator1.NumberOfCalculatedPoints = 100;
            DateTime MonteCarlo100Starts = DateTime.Now;
            calculator1.Algorithm = ChanceToBeatAllCalculator.IntegrationAlgorithm.MonteCarlo;
            double rez1MonteCarlo100 = calculator1.CalculateChanceToBeatAll(page1.ConversionPageViewCount, page1.PageViewCount, page2.ConversionPageViewCount, page2.PageViewCount, page3.ConversionPageViewCount, page3.PageViewCount);
            double rez2MonteCarlo100 = calculator1.CalculateChanceToBeatAll(page2.ConversionPageViewCount, page2.PageViewCount, page1.ConversionPageViewCount, page1.PageViewCount, page3.ConversionPageViewCount, page3.PageViewCount);
            double rez3MonteCarlo100 = calculator1.CalculateChanceToBeatAll(page3.ConversionPageViewCount, page3.PageViewCount, page1.ConversionPageViewCount, page1.PageViewCount, page2.ConversionPageViewCount, page2.PageViewCount);

            //calculator1.NumberOfCalculatedPoints = 300;
            DateTime MonteCarlo300Starts = DateTime.Now;
            //calculator1.Algorithm = ChanceToBeatAllCalculator.IntegrationAlgorithm.MonteCarlo;
            //double rez1MonteCarlo300 = calculator1.CalculateChanceToBeatAll(page1.ConversionPageViewCount, page1.PageViewCount, page2.ConversionPageViewCount, page2.PageViewCount, page3.ConversionPageViewCount, page3.PageViewCount);
            //double rez2MonteCarlo300 = calculator1.CalculateChanceToBeatAll(page2.ConversionPageViewCount, page2.PageViewCount, page1.ConversionPageViewCount, page1.PageViewCount, page3.ConversionPageViewCount, page3.PageViewCount);
            //double rez3MonteCarlo300 = calculator1.CalculateChanceToBeatAll(page3.ConversionPageViewCount, page3.PageViewCount, page1.ConversionPageViewCount, page1.PageViewCount, page2.ConversionPageViewCount, page2.PageViewCount);

            calculator1.NumberOfCalculatedPoints = 100;
            calculator1.Algorithm = ChanceToBeatAllCalculator.IntegrationAlgorithm.TrapeziumIntegration;
            DateTime Trapezium100Starts = DateTime.Now;
            double rez1Trapezium100 = calculator1.CalculateChanceToBeatAll(page1.ConversionPageViewCount, page1.PageViewCount, page2.ConversionPageViewCount, page2.PageViewCount, page3.ConversionPageViewCount, page3.PageViewCount);
            double rez2Trapezium100 = calculator1.CalculateChanceToBeatAll(page2.ConversionPageViewCount, page2.PageViewCount, page1.ConversionPageViewCount, page1.PageViewCount, page3.ConversionPageViewCount, page3.PageViewCount);
            double rez3Trapezium100 = calculator1.CalculateChanceToBeatAll(page3.ConversionPageViewCount, page3.PageViewCount, page1.ConversionPageViewCount, page1.PageViewCount, page2.ConversionPageViewCount, page2.PageViewCount);

            //calculator1.NumberOfCalculatedPoints = 300;
            //calculator1.Algorithm = ChanceToBeatAllCalculator.IntegrationAlgorithm.TrapeziumIntegration;
            DateTime Trapezium300Starts = DateTime.Now;            
            //double rez1Trapezium300 = calculator1.CalculateChanceToBeatAll(page1.ConversionPageViewCount, page1.PageViewCount, page2.ConversionPageViewCount, page2.PageViewCount, page3.ConversionPageViewCount, page3.PageViewCount);
            //double rez2Trapezium300 = calculator1.CalculateChanceToBeatAll(page2.ConversionPageViewCount, page2.PageViewCount, page1.ConversionPageViewCount, page1.PageViewCount, page3.ConversionPageViewCount, page3.PageViewCount);
            //double rez3Trapezium300 = calculator1.CalculateChanceToBeatAll(page3.ConversionPageViewCount, page3.PageViewCount, page1.ConversionPageViewCount, page1.PageViewCount, page2.ConversionPageViewCount, page2.PageViewCount);

            DateTime trapezium300stops = DateTime.Now;

            TimeSpan montecarlo100span = new TimeSpan(MonteCarlo300Starts.Ticks - MonteCarlo100Starts.Ticks);
            TimeSpan montecarlo300span = new TimeSpan(Trapezium100Starts.Ticks - MonteCarlo300Starts.Ticks);
            TimeSpan trapezium100span = new TimeSpan(Trapezium300Starts.Ticks - Trapezium100Starts.Ticks);
            TimeSpan trapezium300span = new TimeSpan(trapezium300stops.Ticks - Trapezium300Starts.Ticks);            
                        
            TimeSpan maxSpan = new TimeSpan(0,2,0);
            Assert.IsFalse(montecarlo100span > maxSpan || montecarlo300span > maxSpan || trapezium100span > maxSpan || trapezium100span > maxSpan);

            Assert.IsFalse(rez1MonteCarlo100 > 1 || rez1MonteCarlo100 < 0 || Math.Abs(rez1MonteCarlo100 - rez1Trapezium100) > 0.01);
            Assert.IsFalse(rez2MonteCarlo100 > 1 || rez2MonteCarlo100 < 0 || Math.Abs(rez2MonteCarlo100 - rez2Trapezium100) > 0.01);
            Assert.IsFalse(rez3MonteCarlo100 > 1 || rez3MonteCarlo100 < 0 || Math.Abs(rez3MonteCarlo100 - rez3Trapezium100) > 0.01);

            //Assert.IsFalse(rez1Trapezium100 > 1 || rez1Trapezium100 < 0 || Math.Abs(rez1MonteCarlo300 - rez1Trapezium300) > 0.001);
            //Assert.IsFalse(rez2Trapezium100 > 1 || rez2Trapezium100 < 0 || Math.Abs(rez2MonteCarlo300 - rez2Trapezium300) > 0.001);
            //Assert.IsFalse(rez3Trapezium100 > 1 || rez3Trapezium100 < 0 || Math.Abs(rez3MonteCarlo300 - rez3Trapezium300) > 0.001);

            calculator3.CalculateChanceToBeatAll(111, 112, 222, 21, 121, 21); // should throw exception
        }
	}
}

