using System;

namespace EPiServer.Marketing.KPI.Manager.DataClass
{
    /// <summary>
    /// KeyPerformanceIndicator object that is used to define a test characteristic(i.e. page scroll, page click, etc.)
    /// </summary>
    public class Kpi : IKpi
    {
        public Kpi()
        {
            CreatedDate = DateTime.UtcNow;
            ModifiedDate = DateTime.UtcNow;
        }

        /// <summary>
        /// Id of Kpi.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual string Properties { get; set; }

        /// <summary>
        /// Date the kpi was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The last time the kpi was modified.
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Determines if a conversion has happened.
        /// </summary>
        /// <param name="theValues"></param>
        public virtual bool Evaluate(object theValues)
        {
            throw new NotImplementedException();
        }
    }
}
