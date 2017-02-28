using EPiServer.Marketing.KPI.Commerce.Kpis;
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
    }
}
