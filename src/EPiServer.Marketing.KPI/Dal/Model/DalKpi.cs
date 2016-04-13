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

        /// <summary>
        /// The AssemblyFullyQaulified info for a Kpi object.  This is parsed out to create a new instance of said kpi from the db.
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// Json serialized string storing all necessary properties of a Kpi.
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
