using System;
using System.Runtime.Serialization;
using EPiServer.Marketing.KPI.Dal.Model.Enums;

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
        /// Name of Kpi.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Weight of Kpi relative to others.
        /// </summary>
        [DataMember]
        public int Weight { get; set; }

        /// <summary>
        /// The condition to be met for the kpi to be completed by a user.
        /// </summary>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Page to send user to if they meet the kpi requirement.
        /// </summary>
        [DataMember]
        public Guid LandingPage { get; set; }

        /// <summary>
        /// Indicates whether this Kpi is run on the server side or client side.
        /// </summary>
        public RunAt RunAt { get; set; }

        /// <summary>
        /// Paths to client scripts.  Single string that is comma deliminated.
        /// </summary>
        public string ClientScripts { get; set; }

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
