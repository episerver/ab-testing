using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Markets;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace EPiServer.Marketing.KPI.Commerce.Test
{
    [ExcludeFromCodeCoverage]
    public class CommerceKpiTestsBase
    {
        public Mock<IServiceLocator> _mockServiceLocator = new Mock<IServiceLocator>();
        public Mock<IContentLoader> _mockContentLoader = new Mock<IContentLoader>();
        public Mock<ReferenceConverter> _mockReferenceConverter;
        public Mock<IOrderGroup> _mockOrderGroup = new Mock<IOrderGroup>();
        public Mock<IOrderGroupTotalsCalculator> _mockOrderGroupTotalsCalculator = new Mock<IOrderGroupTotalsCalculator>();
        public Mock<IContentRepository> _mockContentRepository = new Mock<IContentRepository>();
        public Mock<IContentVersionRepository> _mockContentVersionRepository = new Mock<IContentVersionRepository>();
        public Mock<IMarketService> _mockMarketService = new Mock<IMarketService>();
        public Mock<IKpiManager> _mockKpiManager = new Mock<IKpiManager>();
        public Mock<IMarket> _mockMarket = new Mock<IMarket>();
        public Mock<ILogger> _logger = new Mock<ILogger>();


    }
}
