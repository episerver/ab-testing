using System;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using System.Globalization;

namespace EPiServer.Marketing.KPI.Manager
{
    public class CommerceData : IDynamicData
    {
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
