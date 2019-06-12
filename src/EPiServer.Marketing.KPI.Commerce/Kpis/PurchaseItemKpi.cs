using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Results;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using EPiServer.Commerce.Catalog.Linking;
using EPiServer.Commerce.Order;

namespace EPiServer.Marketing.KPI.Commerce.Kpis
{
    /// <summary>
    /// Measures how many users purchase a specific item. Results: Views correlate to visitors that viewed the item, while conversions represent purchasing of said item.
    /// </summary>
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Commerce.Markup.ProductPickerConfigMarkup.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Commerce.Markup.ProductPickerReadOnlyMarkup.html",
        text_id = "/commercekpi/purchaseitem/name",
        description_id = "/commercekpi/purchaseitem/description")]
    public class PurchaseItemKpi : CommerceKpi
    {
        [ExcludeFromCodeCoverage]
        public PurchaseItemKpi()
        {
            LocalizationSection = "purchaseitem";
        }

        [ExcludeFromCodeCoverage]
        internal PurchaseItemKpi(IServiceLocator serivceLocator)
        {
            LocalizationSection = "purchaseitem";
            _servicelocator = serivceLocator;
        }

        /// <inheritdoc />
        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            var retval = false;

            var contentLoader = _servicelocator.GetInstance<IContentLoader>();
            var referenceConverter = _servicelocator.GetInstance<ReferenceConverter>();

            var ea = e as OrderGroupEventArgs;
            var ordergroup = sender as IPurchaseOrder;
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
