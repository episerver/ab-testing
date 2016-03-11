using System;
using EPiServer.Marketing.KPI.Dal.Model.Enums;

namespace EPiServer.Marketing.KPI.Dal.Model
{
    /// <summary>
    /// Interface for KPI objects.
    /// </summary>
    public interface IKpi
    {
        /// <summary>
        /// Id of Kpi.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Name of Kpi. 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Wieght of kpi compared to other kpis.
        /// </summary>
        int Weight { get; set; }

        /// <summary>
        /// The condition to be met for the kpi to be met by a user.
        /// </summary>
        string Value { get; set; }

        /// <summary>
        /// Page to direct user to upon successful conversion.
        /// </summary>
        Guid LandingPage { get; set; }

        /// <summary>
        /// Indicates whether this Kpi is run on the server side or client side.
        /// </summary>
        RunAt RunAt { get; set; }

        /// <summary>
        /// Paths to client scripts.  Single string that is comma deliminated.
        /// </summary>
        string ClientScripts { get; set; }

        /// <summary>
        /// Date the kpi was created.
        /// </summary>
        DateTime CreatedDate { get; set; }

        /// <summary>
        /// The last time the kpi was modified.
        /// </summary>
        DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Determines if a conversion has happened.
        /// </summary>
        /// <param name="theValues"></param>
        void Success(object theValues);
    }
}
