using System;
using EPiServer.Marketing.Testing.KPI.Model.Enums;

namespace EPiServer.Marketing.Testing.KPI.Model
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
    }
}
