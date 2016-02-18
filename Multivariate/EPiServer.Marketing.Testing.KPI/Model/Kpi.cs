using System;
using System.Runtime.Serialization;
using EPiServer.Marketing.Testing.KPI.Model.Enums;

namespace EPiServer.Marketing.Testing.KPI.Model
{
    /// <summary>
    /// KeyPerformanceIndicator object that is used to define a test characteristic(i.e. page scroll, page click, etc.)
    /// </summary>
    [DataContract]
    public class Kpi : IKpi
    {
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
        /// Percentage of visitors that will be part of the test.
        /// </summary>
        [DataMember]
        public int ParticipationPercentage { get; set; }

        /// <summary>
        /// Page to send user to if they meet the kpi requirement.
        /// </summary>
        [DataMember]
        public Guid LandingPage { get; set; }

        /// <summary>
        /// Indicates whether this Kpi is run on the server side or client side.
        /// </summary>
        public RunAt RunAt { get; set; }

    }
}
