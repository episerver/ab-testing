using EPiServer.Commerce.Order;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Results;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Collections.Generic;
using EPiServer.Framework.Localization;

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
        public override string UiMarkup
        {
            get
            {
                var averageordertext = LocalizationService.Current
                   .GetString("/commercekpi/" + LocalizationSection + "/config_markup/averageordertext");
                return string.Format(base.UiMarkup, averageordertext);

            }
        }

        [DataMember]
        public override string UiReadOnlyMarkup
        {
            get
            {
                var averageordertext = LocalizationService.Current
                   .GetString("/commercekpi/" + LocalizationSection + "/readonly_markup/averageordertext");
                return string.Format(base.UiReadOnlyMarkup, averageordertext);

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
                retval.Total = _servicelocator.GetInstance<IOrderGroupTotalsCalculator>().GetTotals(ordergroup).SubTotal.Amount;
            }
            return retval;
        }
    }
}
