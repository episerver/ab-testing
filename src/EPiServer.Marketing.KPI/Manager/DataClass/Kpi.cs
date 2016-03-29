using System;
using System.Runtime.Serialization;
using EPiServer.Marketing.KPI.Manager.DataClass.Enums;

namespace EPiServer.Marketing.KPI.Manager.DataClass
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
        /// 
        /// </summary>
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
        /// Determines if a conversion has happened.
        /// </summary>
        /// <param name="theValues"></param>
        public void Success(object theValues)
        {
            throw new NotImplementedException();
        }
    }
}
