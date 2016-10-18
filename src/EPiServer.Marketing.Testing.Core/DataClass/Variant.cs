using System;
using System.Collections.Generic;
using EPiServer.Marketing.Testing.Core.DataClass;

namespace EPiServer.Marketing.Testing.Data
{
    public class Variant : CoreEntityBase
    {
        /// <summary>
        /// Id of the test this is associated with.
        /// </summary>
        public Guid? TestId { get; set; }

        /// <summary>
        /// Id of a variant to use instead of the original item for a test.
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Version of original item that is selected as a variant.
        /// </summary>
        public int ItemVersion { get; set; }

        /// <summary>
        /// True if variant won the test, false otherwise.
        /// </summary>
        public bool IsWinner { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Conversions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Views { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// Reference to the test this is associated with.
        /// </summary>
        public virtual ABTest ABTest { get; set; }

        public IList<KeyFinancialResult> KeyFinancialResults { get; set; }

        public IList<KeyValueResult> KeyValueResults { get; set; }
    }
}
