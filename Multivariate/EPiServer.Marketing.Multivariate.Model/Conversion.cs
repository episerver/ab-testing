using System;

namespace EPiServer.Marketing.Multivariate.Model
{
    public class Conversion : EntityBase
    {
        /// <summary>
        /// 
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Id of the test this conversion is associated with.
        /// </summary>
        public Guid TestId { get; set; }

        /// <summary>
        /// String that represents the conversion.  Still needs to be figured out how to represent this...
        /// </summary>
        public string ConversionString { get; set; }

        /// <summary>
        /// Reference to the test this is associated with.
        /// </summary>
        public virtual MultivariateTest MultivariateTest { get; set; }
    }
}
