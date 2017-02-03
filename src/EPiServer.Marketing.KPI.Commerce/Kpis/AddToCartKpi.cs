using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Results;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using System;
using System.Runtime.Serialization;
using System.Linq;

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
        public AddToCartKpi()
        {
            LocalizationSection = "addtocart";
        }

        /// <summary>
        /// Called when we are expected to evaluate. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            var retval = false;
                
            var contentLoader = _servicelocator.GetInstance<IContentLoader>();
            var referenceConverter = _servicelocator.GetInstance<ReferenceConverter>();

            var ea = e as OrderGroupEventArgs;
            var ordergroup = sender as OrderGroup;
            if (ea != null && ordergroup != null)
            {
                foreach (var o in ordergroup.OrderForms.ToArray())
                {
                    foreach( var lineitem in o.LineItems.ToArray())
                    {
                        //We use the content link builder to get the contentlink to our product
                        var productLink = referenceConverter.GetContentLink(lineitem.Code);

                        //Get the product using CMS API
                        var productContent = contentLoader.Get<CatalogContentBase>(productLink);

                        //The commerce content name represents the name of the product
                        var productName = productContent.Name;

                        // if we are looking for an exact match at the entry level, 
                        // we can just check the Guid
                        if (isVariant)
                        {
                            retval = ContentGuid.Equals(productContent.ContentGuid);
                            if (retval)
                            {
                                break;
                            }
                        }
                        else
                        {
                            // else we can assume its a product variant
                            var repository = _servicelocator.GetInstance<IContentRepository>();
                            var variant = repository.Get<VariationContent>(productLink);
                            var parentProductRef = variant.GetParentProducts().FirstOrDefault();
                            if (parentProductRef != null)
                            {
                                //Get the parent product using CMS API
                                var parentProduct = contentLoader.Get<CatalogContentBase>(parentProductRef);
                                retval = ContentGuid.Equals(parentProduct.ContentGuid);
                                if (retval)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return new KpiConversionResult() { KpiId = Id, HasConverted = retval };
        }
    }
}
