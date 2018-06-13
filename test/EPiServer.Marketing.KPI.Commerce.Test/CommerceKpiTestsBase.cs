using EPiServer.Commerce.Order;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Markets;
using Moq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace EPiServer.Marketing.KPI.Commerce.Test
{
    [ExcludeFromCodeCoverage]
    public class CommerceKpiTestsBase
    {
        //Mocked Service Locator
        public Mock<IServiceLocator> _mockServiceLocator = new Mock<IServiceLocator>();

        //Mocked Content Objects
        public Mock<IContentLoader> _mockContentLoader = new Mock<IContentLoader>();
        public Mock<IContentRepository> _mockContentRepository = new Mock<IContentRepository>();
        public Mock<IContentVersionRepository> _mockContentVersionRepository = new Mock<IContentVersionRepository>();

        //Mocked Commerce/Market Objects
        public Mock<IMarketService> _mockMarketService = new Mock<IMarketService>();
        public Mock<IMarket> _mockMarket = new Mock<IMarket>();
        public Mock<IOrderGroup> _mockOrderGroup = new Mock<IOrderGroup>();
        public Mock<IOrderGroupTotalsCalculator> _mockOrderGroupTotalsCalculator = new Mock<IOrderGroupTotalsCalculator>();

        public Mock<ReferenceConverter> _mockReferenceConverter;

        //Mocked AB Objects
        public Mock<IKpiManager> _mockKpiManager = new Mock<IKpiManager>();

        public class MyLogger : ILogger
        {
            public bool ErrorCalled;
            public bool WarningCalled;
            public bool IsEnabled(Level level)
            {
                return true;
            }

            public void Log<TState, TException>(Level level, TState state, TException exception, Func<TState, TException, string> messageFormatter, Type boundaryType) where TException : Exception
            {
                if (level == Level.Error)
                {
                    ErrorCalled = true;
                }
                else if (level == Level.Warning)
                {
                    WarningCalled = true;
                }
            }
        }
    }
}
