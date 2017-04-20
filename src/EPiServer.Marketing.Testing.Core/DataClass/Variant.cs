using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.Testing.Core.DataClass
{
    /// <summary>
    /// Object that stores all info related to an item under test.
    /// </summary>
    public class Variant : CoreEntityBase
    {
        /// <summary>
        /// Id of the test this variant is associated with.
        /// </summary>
        public Guid? TestId { get; set; }

        /// <summary>
        /// Id of modified content to use instead of the original item for a test.
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Version of the content.
        /// </summary>
        public int ItemVersion { get; set; }

        /// <summary>
        /// This is calculated and updated upon the completeion of a test or when a winner is chosen.  
        /// True if variant won the test, false otherwise.
        /// </summary>
        public bool IsWinner { get; set; }

        /// <summary>
        /// A running count of how many users have converted after viewing the item under test.
        /// </summary>
        public int Conversions { get; set; }

        /// <summary>
        /// A running count of how many users have viewed the item under test.
        /// </summary>
        public int Views { get; set; }

        /// <summary>
        /// States whether this version of the content is the currently puublished one or not at the start of the test.
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// Reference to the test this is associated with.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public virtual ABTest ABTest { get; set; }

        /// <summary>
        /// A list of any financial results that are the result of a kpi associated with the test this variant is part of.
        /// </summary>
        public IList<KeyFinancialResult> KeyFinancialResults { get; set; }

        /// <summary>
        /// A list of any value results(i.e. any numerical value that is not fiancial in nature) that are the result of a kpi associated with the test this variant is part of.
        /// </summary>
        public IList<KeyValueResult> KeyValueResults { get; set; }
    }
}
