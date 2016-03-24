using System;
using System.Runtime.Serialization;

namespace EPiServer.Marketing.KPI.Dal.Model
{
    /// <summary>
    /// KeyPerformanceIndicator object that is used to define a test characteristic(i.e. page scroll, page click, etc.)
    /// </summary>
    [DataContract]
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
        [DataMember]
        public Guid Id { get; set; }

        /// <summary>
        /// The condition to be met for the kpi to be completed by a user.
        /// </summary>
        [DataMember]
        public string Properties { get; set; }

        /// <summary>
        /// Date the kpi was created.
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The last time the kpi was modified.
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
