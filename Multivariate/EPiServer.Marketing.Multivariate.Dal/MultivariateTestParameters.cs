using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Multivariate.Dal
{
    public class MultivariateTestParameters
    {
        /// <summary> Optional </summary>
        public Guid Id { get; set; }

        /// <summary> Required </summary>
        public string Title { get; set; }

        /// <summary> Required </summary>
        public string Owner { get; set; }

        /// <summary> Required </summary>
        public string State { get; set; }

        /// <summary> Required </summary>
        public DateTime StartDate { get; set; }

        /// <summary> Optional </summary>
        public DateTime EndDate { get; set; }

        /// <summary> Required </summary>
        public Guid OriginalItemId { get; set; }

        /// <summary> Required </summary>
        public List<Guid> VariantItems { get; set; }

        /// <summary> Optional </summary>
        public DateTime LastModifiedDate { get; set; }

        /// <summary> Optional </summary>
        public string LastModifiedBy { get; set; }

        /// <summary> Optional </summary>
        public List<Result> Results { get; set; }

        public List<PerformanceIndicator> Conversions { get; set; }

        /// <summary> Required </summary>
        public Guid VariantItemId { get; set; }

        /// <summary> Required </summary>
        public Guid ConversionItemId { get; set; }
    }

    public class Result
    {
        public Guid ItemId { get; set; }
        public int Views { get; set; }
        public int Conversions { get; set; }
    }

    public class PerformanceIndicator
    {
        
    }

       
}
