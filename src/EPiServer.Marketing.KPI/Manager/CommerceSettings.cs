using System;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using System.Globalization;

namespace EPiServer.Marketing.KPI.Manager
{
    public class CommerceSettings : IDynamicData
    {
        public Identity Id { get; set; }
       
        public string PreferredMarketValue { get; set; }

        public NumberFormatInfo preferredFormat { get; set; }
    }
}
