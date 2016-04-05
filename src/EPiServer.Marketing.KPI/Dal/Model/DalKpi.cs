using System;

namespace EPiServer.Marketing.KPI.Dal.Model
{
    /// <summary>
    /// KeyPerformanceIndicator object that is used to define a test characteristic(i.e. page scroll, page click, etc.)
    /// </summary>
    public class DalKpi : IDalKpi
    {
        public DalKpi()
        {
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Id of DalKpi.
        /// </summary>
        public Guid Id { get; set; }

        public string ClassName { get; set; }

        /// <summary>
        /// The condition to be met for the DalKpi to be completed by a user.
        /// </summary>
        public string Properties { get; set; }

        /// <summary>
        /// Date the DalKpi was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The last time the DalKpi was modified.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Evaluates if a conversion has happened.
        /// </summary>
        /// <param name="theValues"></param>
        public bool Evaluate(object theValues)
        {
            throw new NotImplementedException();
        }
    }
}
