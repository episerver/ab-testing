using System.Collections;
using System.Collections.Generic;
using EPiServer.Commerce.Marketing;
using EPiServer.Commerce.Order;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;

namespace EPiServer.Marketing.KPI.Commerce.Test.Fakes
{
    public class FakeOrderForm : IOrderForm
    {
        private readonly int _orderFormId;
        private static int _counter;
        private readonly IList<PromotionInformation> _promotions = new List<PromotionInformation>();
        private readonly ICollection<string> _couponCodes = new List<string>();

        public FakeOrderForm()
        {
            Shipments = new List<IShipment>();
            _orderFormId = ++_counter;
            Properties = new Hashtable();
            Payments = new List<IPayment>();
        }

        public string Status { get; set; }

        public decimal AuthorizedPaymentTotal { get; set; }

        public decimal CapturedPaymentTotal { get; set; }

        public decimal HandlingTotal { get; set; }
        public string Name { get; set; }

        public ICollection<IShipment> Shipments { get; set; }

        public int OrderFormId
        {
            get { return _orderFormId; }
        }

        public MarketId MarketId { get; set; }

        public IMarket Market
        {
            get { return new MarketImpl(new MarketId("US")); }
        }

        public IList<PromotionInformation> Promotions
        {
            get { return _promotions; }
        }

        public ICollection<string> CouponCodes
        {
            get { return _couponCodes; }
        }

        public ICollection<IPayment> Payments { get; set; }

        public Hashtable Properties { get; private set; }

        public bool PricesIncludeTax => throw new System.NotImplementedException();
        public IOrderGroup ParentOrderGroup { get; }

        public static FakeOrderForm CreateOrderForm(Hashtable properties = null)
        {
            return new FakeOrderForm
            {
                AuthorizedPaymentTotal = 0,
                CapturedPaymentTotal = 0,
                HandlingTotal = 10,
                Status = "Word",
                MarketId = MarketId.Default,
                Properties = properties ?? new Hashtable()
            };
        }
    }
}
