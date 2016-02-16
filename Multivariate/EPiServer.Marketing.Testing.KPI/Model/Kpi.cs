using System;
using System.Runtime.Serialization;

namespace EPiServer.Marketing.Testing.KPI.Model
{
    [DataContract]
    public class Kpi
    {
        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Weight { get; set; }

        [DataMember]
        public string Value { get; set; }

        [DataMember]
        public int ParticipationPercentage { get; set; }

        [DataMember]
        public Guid ConversionPage { get; set; }

    }
}
