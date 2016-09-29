using System;
using System.Runtime.Serialization;

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

        [DataMember]
        public string FriendlyName => "Generic KPI";

        [DataMember]
        public string UiMarkup => "I am a generic KPI";

        /// <summary>
        /// Date the kpi was created.
        /// </summary>
        [DataMember]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The last time the kpi was modified.
        /// </summary>
        [DataMember]
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Determines if a conversion has happened.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event Argument</param>
        public virtual bool Evaluate(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
