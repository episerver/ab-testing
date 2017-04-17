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
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Manager.DataClass;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.KPI.Commerce.Kpis
{
    /// <summary>
    /// Tests the potential effects of content on the average cart total of participating users.  Results: The total represents the average cart total across all visitors that checked out as part of the test
    /// </summary>
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Commerce.Markup.AverageOrderKpiConfigMarkup.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Commerce.Markup.AverageOrderKpiReadOnlyMarkup.html",
        text_id = "/commercekpi/averageorder/name",
        description_id = "/commercekpi/averageorder/description")]
    [AlwaysEvaluate]
    public class AverageOrderKpi : CommerceKpi, IFinancialKpi
    {
        private ILogger _logger;

        [ExcludeFromCodeCoverage]
        public AverageOrderKpi()
        {
            LocalizationSection = "averageorder";
            _servicelocator = ServiceLocator.Current;
            _logger = LogManager.GetLogger();
        }

        [ExcludeFromCodeCoverage]
        internal AverageOrderKpi(IServiceLocator servicelocator)
        {
            LocalizationSection = "averageorder";
            _servicelocator = servicelocator;
        }

        /// <inheritdoc />
        [DataMember]
        public override string KpiResultType
        {
            get
            {
                return typeof(KpiFinancialResult).Name.ToString();
            }
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        [DataMember]
        public override string UiReadOnlyMarkup
        {
            get
            {
                var conversionText = LocalizationService.Current
                   .GetString("/commercekpi/" + LocalizationSection + "/readonly_markup/conversion_description");
                return string.Format(base.UiReadOnlyMarkup, conversionText);
            }
        }

        [DataMember]
        public CommerceData PreferredFinancialFormat { get; set; }       

        /// <inheritdoc />
        public override void Validate(Dictionary<string, string> responseData)
        {
            var marketService = _servicelocator.GetInstance<IMarketService>();
            var kpiManager = _servicelocator.GetInstance<IKpiManager>();

            var commerceData = kpiManager.GetCommerceSettings();

            if(commerceData == null)
            {
                var defaultMarket = marketService.GetMarket("DEFAULT");
                if(defaultMarket == null)
                {
                    throw new KpiValidationException(LocalizationService.Current.GetString("/commercekpi/averageorder/config_markup/error_defaultmarketundefined"));
;                }
            }
            else
            {
                var preferredMarket = marketService.GetMarket(commerceData.CommerceCulture);
                if(preferredMarket == null)
                {
                    throw new KpiValidationException(LocalizationService.Current.GetString("/commercekpi/averageorder/config_markup/error_undefinedmarket"));
                }
            }
        }

        /// <inheritdoc />
        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            var retval = new KpiFinancialResult() {
                KpiId = Id,
                Total = 0,
                HasConverted = false
            };

            var ordergroup = sender as IPurchaseOrder;
            if (ordergroup != null)
            {
                var orderTotal = _servicelocator.GetInstance<IOrderGroupTotalsCalculator>().GetTotals(ordergroup).SubTotal;
                var orderMarket = _servicelocator.GetInstance<IMarketService>().GetMarket(ordergroup.Market.MarketId);
                var orderCurrency = orderMarket.DefaultCurrency.CurrencyCode;
                var preferredMarket = _servicelocator.GetInstance<IMarketService>().GetMarket(PreferredFinancialFormat.CommerceCulture);

                if (preferredMarket != null)
                {
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
                else
                {
                    _logger.Error(LocalizationService.Current.GetString("/commercekpi/averageorder/config_markup/error_undefinedmarket"));
                }
                
            }
            return retval;
        }
    }
}
