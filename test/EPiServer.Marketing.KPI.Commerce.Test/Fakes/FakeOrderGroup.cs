using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Commerce.Order;
using EPiServer.Marketing.KPI.Commerce.Test.Fakes;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Orders;

namespace EPiServer.Marketing.Testing.Test.Fakes
{
    public class FakeOrderGroup : ICart
    {
        private Guid _customerId = Guid.NewGuid();
        private static int _counter;
        private OrderGroupTotals _orderGroupTotals;

        public FakeOrderGroup()
        {
            Forms = new List<IOrderForm>();
            Market = new MarketImpl(MarketId.Default);
            OrderLink = new OrderReference(++_counter, "Default", _customerId, typeof(Cart));
            Properties = new Hashtable();
            Notes = new List<IOrderNote>();
        }

        public OrderReference OrderLink { get; set; }

        public ICollection<IOrderForm> Forms { get; set; }

        public IMarket Market { get; set; }
        public ICollection<IOrderNote> Notes { get; }
        public string Name { get; set; }
        public Guid? Organization { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public Hashtable Properties { get; private set; }

        public static FakeOrderGroup CreateOrderGroup(ICollection<FakeOrderForm> forms, Hashtable properties = null)
        {
            return new FakeOrderGroup
            {
                Market = new MarketImpl(MarketId.Default),
                Forms = forms.Cast<IOrderForm>().ToList(),
                Properties = properties ?? new Hashtable()
            };
        }

        public void AddOrderGroupTotals(OrderGroupTotals orderGroupTotal)
        {
            _orderGroupTotals = orderGroupTotal;
        }

        public Currency Currency { get; set; }

        public Guid CustomerId
        {
            get
            {
                return _customerId;
            }
            set
            {
                _customerId = value;
            }
        }

        public DateTime Created => DateTime.MaxValue;

        public DateTime? Modified => null;

        public MarketId MarketId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string MarketName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool PricesIncludeTax { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public OrderGroupTotals GetTotals()
        {
            return _orderGroupTotals;
        }

    }
}
