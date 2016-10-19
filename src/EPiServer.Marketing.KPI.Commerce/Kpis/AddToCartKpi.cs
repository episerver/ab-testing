using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using Mediachase.Commerce.Orders;
using System;
using System.Runtime.Serialization;

namespace EPiServer.Marketing.KPI.Commerce.Kpis
{
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Commerce.Markup.AddToCartConfigMarkup.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Commerce.Markup.AddToCartReadOnlyMarkup.html",
        text = "Product", description = "Choose a product for conversion.")]
    public class AddToCartKpi : Kpi
    {
        /// <summary>
        /// Called when we are expected to evaluate. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            var retval = false;
            var ea = e as OrderGroupEventArgs;
            var ordergroup = sender as OrderGroup;
            if (ea != null && ordergroup != null)
            {
                // todo, figure out how we convert based on what we have.
                // note that the order group has actual info in it, not the eventargs.
                //
                /* Some sample code we might need.
                           foreach (var o in ordergroup.OrderForms.ToArray())
                           {
                               var a = o.LineItems.ToArray();
                           }
                  
                 */
                retval = false; 
            }

            return new KpiConversionResult() { KpiId = Id, HasConverted = retval };
        }

        private EventHandler<OrderGroupEventArgs> _eh;
        /// <summary>
        /// Setup the event that we want to be evaluated on
        /// </summary>
        public override event EventHandler EvaluateProxyEvent
        {
            add
            {
                _eh = new EventHandler<OrderGroupEventArgs>(value);
                OrderContext.Current.OrderGroupUpdated += _eh;
            }
            remove
            {
                OrderContext.Current.OrderGroupUpdated -= _eh;
            }
        }
    }
}
