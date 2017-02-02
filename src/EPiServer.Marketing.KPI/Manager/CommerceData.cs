using System;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using System.Globalization;

namespace EPiServer.Marketing.KPI.Manager
{
    /// <summary>
    /// Kpi implentation for using the DynamicDataStore that is part of EPiServer for storing commerce related settings.
    /// </summary>
    public class CommerceData : IDynamicData
    {
        /// <summary>
        /// The EPiServer.Data.Identity of the item.
        /// </summary>
        public Identity Id { get; set; }
       
        /// <summary>
        /// String representing the commerce "market" to be used for financial conversions
        /// "Default" represents the default market as defined in the system.
        /// </summary>
        public string CommerceCulture { get; set; }

        /// <summary>
        /// Details about the preferred format to display numbers, specifically currencies.        /// 
        /// </summary>
        public NumberFormatInfo preferredFormat { get; set; }
    }
}
