using System;
using EPiServer.Marketing.KPI.Results;
using Xunit;

namespace EPiServer.Marketing.KPI.Test
{
    public class KpiResultTests
    {

        [Fact]
        public void Kpi_ConversionResult_Test()
        {
            var id = Guid.NewGuid();
            var result = new KpiConversionResult() {KpiId = id, HasConverted = true};

            Assert.Equal(id, result.KpiId);
            Assert.Equal(true, result.HasConverted);
        }

        [Fact]
        public void Kpi_FinancialResult_Test()
        {
            var id = Guid.NewGuid();

            var financialResult = new KpiFinancialResult()
            {
                KpiId = id,
                ConvertedTotal = (decimal)2.3,
                ConvertedTotalCulture = "en",
                HasConverted = true,
                Total = (decimal)3.2,
                TotalMarketCulture = "en"
            };

            Assert.Equal((decimal)2.3, financialResult.ConvertedTotal);
            Assert.Equal("en", financialResult.TotalMarketCulture);
            Assert.Equal((decimal)3.2, financialResult.Total);
            Assert.Equal(id, financialResult.KpiId);
            Assert.Equal(true, financialResult.HasConverted);
            Assert.Equal("en", financialResult.ConvertedTotalCulture);
        }

        [Fact]
        public void Kpi_ValueResult_Test()
        {
            var id = Guid.NewGuid();

            var valueResult = new KpiValueResult()
            {
                KpiId = id,
                HasConverted = false,
                Value = 3.2
            };

            Assert.Equal(3.2, valueResult.Value);
            Assert.Equal(id, valueResult.KpiId);
            Assert.Equal(false, valueResult.HasConverted);

        }
    }
}
