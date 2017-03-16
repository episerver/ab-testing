using System.Collections;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using Mediachase.Commerce.Inventory;

namespace EPiServer.Marketing.KPI.Commerce.Test.Fakes
{
    public class FakeLineItem : ILineItem, ILineItemDiscountAmount
    {
        private static int _counter;

        public FakeLineItem()
        {
            LineItemId = ++_counter;
            Properties = new Hashtable();
        }

        public int LineItemId { get; set; }

        public string Code { get; set; }

        public decimal Quantity { get; set; }
        public decimal ReturnQuantity { get; set; }
        public InventoryTrackingStatus InventoryTrackingStatus { get; set; }
        public bool IsInventoryAllocated { get; set; }

        public decimal LineItemDiscountAmount { get; set; }

        public decimal PlacedPrice { get; set; }

        public decimal OrderLevelDiscountAmount { get; set; }

        public string DisplayName { get; set; }

        public bool IsGift { get; set; }

        public static FakeLineItem CreateLineItem(string code, decimal price, decimal quantity, decimal lineItemDiscount = 0, bool isGift = false)
        {
            return CreateLineItem(++_counter, code, price, quantity, lineItemDiscount, isGift: isGift);
        }

        public static FakeLineItem CreateLineItem(int id, string code, decimal price, decimal quantity, decimal lineItemDiscount = 0, decimal orderLevelDiscount = 0,
            bool isGift = false, Hashtable properties = null, decimal returnQuantity = 0, string displayName = null)
        {

            var fakeLineItem = new FakeLineItem
            {
                Code = code,
                LineItemDiscountAmount = lineItemDiscount,
                OrderLevelDiscountAmount = orderLevelDiscount,
                LineItemId = id,
                PlacedPrice = price,
                Quantity = quantity,
                IsGift = isGift,
                Properties = properties ?? new Hashtable(),
                ReturnQuantity = returnQuantity,
                DisplayName = displayName
            };

            fakeLineItem.TrySetDiscountValue(x => x.EntryAmount, fakeLineItem.LineItemDiscountAmount);
            fakeLineItem.TrySetDiscountValue(x => x.OrderAmount, fakeLineItem.OrderLevelDiscountAmount);

            return fakeLineItem;
        }

        public Hashtable Properties { get; private set; }

        decimal ILineItemDiscountAmount.EntryAmount
        {
            get
            {
                return LineItemDiscountAmount;
            }
            set
            {
                LineItemDiscountAmount = value;
            }
        }

        /// <summary>
        /// Gets or sets the order level discount amount.
        /// </summary>
        /// <value>The order level discount amount.</value>
        decimal ILineItemDiscountAmount.OrderAmount
        {
            get
            {
                return OrderLevelDiscountAmount;
            }
            set
            {
                OrderLevelDiscountAmount = value;
            }
        }
    }
}
