using System;
using System.Collections.Generic;
using EPiServer.Commerce.Order;
using EPiServer.Marketing.Testing.Test.Fakes;

namespace EPiServer.Marketing.KPI.Commerce.Test.Fakes
{
    public class FakePurchaseOrder : FakeOrderGroup, IPurchaseOrder
    {
        public FakePurchaseOrder()
        {
            ReturnForms = new List<IOrderForm>();
        }

        public string OrderNumber { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public ICollection<IOrderForm> ReturnForms { get; private set; }
    }
}
