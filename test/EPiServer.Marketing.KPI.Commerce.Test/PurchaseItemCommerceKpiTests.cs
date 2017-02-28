﻿using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Framework.Cache;
using EPiServer.Marketing.KPI.Commerce.Kpis;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.Marketing.KPI.Commerce.Test
{
    [ExcludeFromCodeCoverage]
    public class PurchaseItemCommerceKpiTests : CommerceKpiTestsBase
    {
        private Guid _kpiId = Guid.Parse("c1327f8f-4063-48b0-a35a-61b9a37d3901");


        private PurchaseItemKpi GetUnitUnderTest()
        {
            var synchronizedObjectInstanceCache = new Mock<ISynchronizedObjectInstanceCache>();
            _mockReferenceConverter = new Mock<ReferenceConverter>(
                new EntryIdentityResolver(synchronizedObjectInstanceCache.Object),
                new NodeIdentityResolver(synchronizedObjectInstanceCache.Object));

            _mockServiceLocator.Setup(loc => loc.GetInstance<IContentLoader>()).Returns(_mockContentLoader.Object);
            _mockServiceLocator.Setup(loc => loc.GetInstance<ReferenceConverter>()).Returns(_mockReferenceConverter.Object);
            _mockServiceLocator.Setup(loc => loc.GetInstance<IContentRepository>()).Returns(_mockContentRepository.Object);

            return new PurchaseItemKpi(_mockServiceLocator.Object);
        }

        [Fact]
        public void PurchaseItemCommerceKpiResultType_Is_KpiFinancialresult()
        {
            var PurchaseItemKpi = GetUnitUnderTest();
            Assert.True(PurchaseItemKpi.KpiResultType == "KpiConversionResult");
        }

        [Fact]
        public void PurchaseItem_UIMarkup_IsRetreived_Correctly()
        {
            var purchaseItemKpi = GetUnitUnderTest();
            Assert.NotNull(purchaseItemKpi.UiMarkup);
        }

        [Fact]
        public void PurchaseItem_UIReadOnlyMarkup_IsRetrieved_Correctly()
        {
            var purchaseItemKpi = GetUnitUnderTest();
            Assert.NotNull(purchaseItemKpi.UiReadOnlyMarkup);
        }

        [Fact]
        public void AddToCart_DefaultReturnVal_Returned_WhenOrderGroupEventArgsAndOrderGroup_AreNull()
        {
            PurchaseItemKpi purchaseItemKpi = GetUnitUnderTest();
            purchaseItemKpi.Id = _kpiId;
            var returnVal = purchaseItemKpi.Evaluate(new object(), new EventArgs());
            Assert.True(returnVal.KpiId == purchaseItemKpi.Id);
            Assert.True(!returnVal.HasConverted);
        }

        [Fact]
        public void AddToCart_DefaultReturnVal_Returned_WhenOrderFormsIsEmpty()
        {
            OrderGroupEventArgs orderArgs = new OrderGroupEventArgs(1, OrderGroupEventType.Cart);
            PurchaseOrder po = new PurchaseOrder(Guid.Parse("0fa0ac0c-25a0-4641-8929-f61b71f15ad2"));
            PurchaseItemKpi purchaseItemKpi = GetUnitUnderTest();
            purchaseItemKpi.Id = _kpiId;
            var returnVal = purchaseItemKpi.Evaluate(po, orderArgs);
            Assert.True(returnVal.KpiId == purchaseItemKpi.Id);
            Assert.True(!returnVal.HasConverted);
        }

        [Fact]
        public void AddToCart_DefaultReturnVal_Returned_WhenLineItemsIsEmpty()
        {
            OrderGroupEventArgs orderArgs = new OrderGroupEventArgs(1, OrderGroupEventType.Cart);
            PurchaseOrder po = new PurchaseOrder(Guid.Parse("0fa0ac0c-25a0-4641-8929-f61b71f15ad2"));
            OrderForm of = new OrderForm();
            OrderForm of2 = new OrderForm();
            po.OrderForms.Add(of);
            po.OrderForms.Add(of2);
            PurchaseItemKpi purchaseItemKpi = GetUnitUnderTest();
            purchaseItemKpi.Id = _kpiId;
            var returnVal = purchaseItemKpi.Evaluate(po, orderArgs);
            Assert.True(returnVal.KpiId == purchaseItemKpi.Id);
            Assert.True(!returnVal.HasConverted);
        }

        [Fact]
        public void AddToCart_HasConverted_IsTrue_WhenIsVariant_AndContentGuidsMatch()
        {
            Guid contentGuid = Guid.Parse("a94daef6-aaad-4d41-a4d9-711f2b441124");

            Mock<CatalogContentBase> catBase = new Mock<CatalogContentBase>();
            catBase.SetupGet(x => x.Name).Returns("Mock Catalog Content");
            catBase.SetupGet(x => x.ContentGuid).Returns(contentGuid);


            Core.ContentReference refer = new Core.ContentReference() { ID = 1, WorkID = 111 };
            OrderGroupEventArgs orderArgs = new OrderGroupEventArgs(1, OrderGroupEventType.Cart);
            PurchaseOrder po = new PurchaseOrder(Guid.Parse("0fa0ac0c-25a0-4641-8929-f61b71f15ad2"));
            OrderForm of = new OrderForm();
            of.LineItems.Add(new LineItem() { ListPrice = 5.95M, Code = "linecode" });
            po.OrderForms.Add(of);

            PurchaseItemKpi purchaseItemKpi = GetUnitUnderTest();

            _mockReferenceConverter.Setup(call => call.GetContentLink(It.IsAny<string>())).Returns(refer);
            _mockContentLoader.Setup(call => call.Get<CatalogContentBase>(It.IsAny<Core.ContentReference>())).Returns(catBase.Object);

            purchaseItemKpi.Id = _kpiId;
            purchaseItemKpi.ContentGuid = contentGuid;
            purchaseItemKpi.isVariant = true;

            var returnVal = purchaseItemKpi.Evaluate(po, orderArgs);
            Assert.True(returnVal.KpiId == purchaseItemKpi.Id);
            Assert.True(returnVal.HasConverted);
        }
    }
}
