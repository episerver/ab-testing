using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Marketing.Testing.Dal.EntityModel;
using EPiServer.Marketing.Testing.Data;

namespace EPiServer.Marketing.Testing.Core.Statistics
{
    public static class Significance
    {
        // Confidence level and corresponding Z-score used to determine significance
        private static readonly Dictionary<int, double> ZScores = new Dictionary<int, double>() { { 90, 1.645 }, { 95, 1.960 }, { 98, 2.326 }, { 99, 2.576 } };

        //public bool CalculateIsSignificant(int originalViews, int originalConversions, int variantViews, int variantConversions, int confidenceLevel)
        //{
        //    var originalConversionRate = originalConversions/originalViews;
        //    var originalStandardError = Math.Sqrt(originalConversionRate * (1 - originalConversionRate) / originalViews);

        //    var variantConversionRate = variantConversions/variantViews;
        //    var variantStandardError = Math.Sqrt(variantConversionRate * (1 - variantConversionRate) / variantViews);

        //    var standardErrorOfDifference = Math.Sqrt( Math.Pow(originalStandardError, 2) + Math.Pow(variantStandardError, 2) );

        //    var calculatedZScore = (variantConversionRate - originalConversionRate) / standardErrorOfDifference;

        //    return calculatedZScore > ZScores[confidenceLevel];
        //}

        public static bool CalculateIsSignificant(IMarketingTest test)
        {
            var originalConversionRate = test.Variants[0].Conversions / test.Variants[0].Views;
            var originalStandardError = Math.Sqrt(originalConversionRate * (1 - originalConversionRate) / test.Variants[0].Views);

            var variantConversionRate = test.Variants[1].Conversions / test.Variants[1].Views;
            var variantStandardError = Math.Sqrt(variantConversionRate * (1 - variantConversionRate) / test.Variants[1].Views);

            var standardErrorOfDifference = Math.Sqrt(Math.Pow(originalStandardError, 2) + Math.Pow(variantStandardError, 2));

            var calculatedZScore = (variantConversionRate - originalConversionRate) / standardErrorOfDifference;

            return calculatedZScore > ZScores[95];
        }
    }
}
