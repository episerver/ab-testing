using EPiServer.Commerce.Order;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Results;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Orders;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using EPiServer.Framework.Localization;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce;
using System.Globalization;
using Mediachase.Commerce.Shared;

namespace EPiServer.Marketing.KPI.Commerce.Kpis
{
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Commerce.Markup.AverageOrderKpiConfigMarkup.html",
      readonlymarkup = "EPiServer.Marketing.KPI.Commerce.Markup.AverageOrderKpiReadOnlyMarkup.html",
      text = "Average Order", description = "Tests the potential effects of content on the average cart total of participating users.")]
    public class AverageOrderKpi : CommerceKpi
    {
        public AverageOrderKpi()
        {
            LocalizationSection = "averageorder";
            _servicelocator = ServiceLocator.Current;
        }
        internal AverageOrderKpi(IServiceLocator servicelocator)
        {
            LocalizationSection = "averageorder";
            _servicelocator = servicelocator;
        }

        [DataMember]
        public override string kpiResultType
        {
            get
            {
                return typeof(KpiFinancialResult).Name.ToString();
            }
        }  

        [DataMember]
        public override string UiMarkup
        {
            get
            {
                var conversionText = LocalizationService.Current
                   .GetString("/commercekpi/" + LocalizationSection + "/config_markup/conversion_label");
                return string.Format(base.UiMarkup, conversionText);

            }
        }

        [DataMember]
        public override string UiReadOnlyMarkup
        {
            get
            {
                var conversionHeader = LocalizationService.Current
                   .GetString("/commercekpi/" + LocalizationSection + "/readonly_markup/conversion_header");

                var conversionText = LocalizationService.Current
                   .GetString("/commercekpi/" + LocalizationSection + "/readonly_markup/conversion_description");
                return string.Format(base.UiReadOnlyMarkup, conversionHeader, conversionText);
            }
        }

        public override void Validate(Dictionary<string, string> responseData)
        {
            //Average order KPI's do not have a form UI so no data is present to validate.
            ContentGuid = Guid.Empty;
        }

        /// <summary>
        /// Called when we are expected to evaluate. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            var retval = new KpiFinancialResult() { KpiId = Id, Total = 0, HasConverted = false };
            
            var ordergroup = sender as PurchaseOrder;
            if (ordergroup != null)
            {
                var orderTotal = _servicelocator.GetInstance<IOrderGroupTotalsCalculator>().GetTotals(ordergroup).SubTotal;
                var orderMarket = _servicelocator.GetInstance<IMarketService>().GetMarket(ordergroup.MarketId);
                string orderCurrency = orderMarket.DefaultCurrency.CurrencyCode;
                string systemCulturalCurrency = CultureInfo.CurrentCulture.ThreeLetterISOLanguageName;               
                
                if (orderCurrency != defaultCurrency)
                {
                    var convertedTotal = CurrencyFormatter.ConvertCurrency(orderTotal, systemCulturalCurrency);
                    retval.Total = convertedTotal.Amount;
                }
                else
                {
                    retval.Total = orderTotal.Amount;
                }
                retval.HasConverted = true;
            }
            return retval;
        }
    }
}
