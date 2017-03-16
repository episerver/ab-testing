using System.Collections.Generic;
using EPiServer.Commerce.Order;
using EPiServer.Marketing.Testing.Test.Fakes;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;

namespace EPiServer.Marketing.KPI.Commerce.Test.Fakes
{
    /// <summary>
    /// Check for more fakes and insight:  https://stash.ep.se/projects/RA/repos/commerce/browse/EPiServer.Commerce.TestTools/Fakes
    /// </summary>
    public static class FakeHelpers
    {
        public static FakeOrderGroup CreateFakeOrderGroup()
        {
            var subject = new FakeOrderGroup()
            {
                Market = new MarketImpl(new MarketId("Default")),
                Forms = new List<IOrderForm>()
                {
                    new FakeOrderForm()
                    {
                        Shipments = new List<IShipment>()
                        {
                            new FakeShipment()
                            {
                                LineItems = new List<ILineItem>()
                                {
                                    new FakeLineItem()
                                }
                            }
                        }
                    }
                }
            };
            return subject;
        }


        public static FakePurchaseOrder CreateFakePurchaseOrder()
        {
            var subject = new FakePurchaseOrder()
            {
                Market = new MarketImpl(new MarketId("Default")),
                Forms = new List<IOrderForm>()
                {
                    new FakeOrderForm()
                    {
                        Shipments = new List<IShipment>()
                        {
                            new FakeShipment()
                            {
                                LineItems = new List<ILineItem>()
                                {
                                    new FakeLineItem()
                                }
                            }
                        }
                    }
                }
            };
            return subject;
        }
    }
}
