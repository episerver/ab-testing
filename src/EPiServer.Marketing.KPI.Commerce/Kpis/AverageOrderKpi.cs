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
using System.Globalization;
using Mediachase.Commerce.Shared;
using Mediachase.Commerce;

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
        public override string KpiResultType
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
            var retval = new KpiFinancialResult() {
                KpiId = Id,
                Total = 0,
                HasConverted = false
            };

            var ordergroup = sender as PurchaseOrder;
            if (ordergroup != null)
            {
                var orderTotal = _servicelocator.GetInstance<IOrderGroupTotalsCalculator>().GetTotals(ordergroup).SubTotal;
                var orderMarket = _servicelocator.GetInstance<IMarketService>().GetMarket(ordergroup.MarketId);
                var orderCurrency = orderMarket.DefaultCurrency.CurrencyCode;
                var preferredMarket = _servicelocator.GetInstance<IMarketService>().GetMarket(PreferredMarket.PreferredMarketValue);

                if (orderCurrency != preferredMarket.DefaultCurrency.CurrencyCode)
                {
                    var convertedTotal = CurrencyFormatter.ConvertCurrency(orderTotal, preferredMarket.DefaultCurrency.CurrencyCode);
                    retval.ConvertedTotal = convertedTotal.Amount;
                }
                else
                {
                    retval.ConvertedTotal = orderTotal.Amount;
                }
                retval.HasConverted = true;
                retval.Total = orderTotal.Amount;
                retval.TotalMarketCulture = orderCurrency;
                retval.ConvertedTotalCulture = preferredMarket.MarketId.Value;
                
            }
            return retval;
        }
    }
}
