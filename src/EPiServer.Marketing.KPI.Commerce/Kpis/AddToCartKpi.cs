using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Results;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using System;
using System.Runtime.Serialization;
using System.Linq;
using EPiServer.Commerce.Order;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace EPiServer.Marketing.KPI.Commerce.Kpis
{
    /// <summary>
    /// Used to determine when a customer adds a specific item to their shopping cart.  This compares the number of customers that only view an item to those that add it to their cart.
    /// </summary>
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Commerce.Markup.ProductPickerConfigMarkup.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Commerce.Markup.ProductPickerReadOnlyMarkup.html",
        text_id = "/commercekpi/addtocart/name", 
        description_id = "/commercekpi/addtocart/description")]
    public class AddToCartKpi : CommerceKpi
    {
        [ExcludeFromCodeCoverage]
        public AddToCartKpi()
        {
            LocalizationSection = "addtocart";
        }

        [ExcludeFromCodeCoverage]
        internal AddToCartKpi(IServiceLocator serviceLocator)
        {
            LocalizationSection = "addtocart";
            _servicelocator = serviceLocator;
        }

        /// <summary>
        /// Determines if a conversion has happened.  Each KPI will decide this differently based on the sender, event args, and the purpose of the KPI.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">The expected Event Argument which contains the necessary info used to decide if a conversion has occured.</param>
        /// <returns>A result containing the necessary data to record a conversion.</returns>
        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            var retval = false;
                
            var contentLoader = _servicelocator.GetInstance<IContentLoader>();
            var referenceConverter = _servicelocator.GetInstance<ReferenceConverter>();

            var ea = e as OrderGroupEventArgs;
            var ordergroup = sender as IOrderGroup;
            if (ea != null && ordergroup != null)
            {
                var contentLinks =
                    referenceConverter.GetContentLinks(ordergroup.Forms.SelectMany(o => o.GetAllLineItems())
                        .Select(x => x.Code))?.Select(p => p.Value);
                var skus = contentLoader.GetItems(contentLinks, CultureInfo.InvariantCulture).OfType<EntryContentBase>();
                foreach (var sku in skus)
                {
                    // if we are looking for an exact match at the entry level, 
                    // we can just check the Guid
                    if (isVariant)
                    {
                        retval = ContentGuid.Equals(sku.ContentGuid);
                        if (retval)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // else we can assume its a product variant
                        var parentProductRef = sku.GetParentProducts().FirstOrDefault();
                        if (parentProductRef != null)
                        {
                            //Get the parent product using CMS API
                            var parentProduct = contentLoader.Get<EntryContentBase>(parentProductRef);
                            retval = ContentGuid.Equals(parentProduct.ContentGuid);
                            if (retval)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return new KpiConversionResult() { KpiId = Id, HasConverted = retval };
        }
    }
}
