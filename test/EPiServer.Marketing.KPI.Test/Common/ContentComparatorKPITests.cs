using EPiServer.Core;
using EPiServer.Marketing.KPI.Common;
using Moq;
using System;
using Xunit;

namespace EPiServer.Marketing.KPI.Test.Common
{
    public class ContentComparatorKPITests
    {
        private Guid LandingPageGuid = new Guid("A051E0AC-571A-4490-8909-854BA43B8E1E");
        Mock<IContent> _content = new Mock<IContent>();

        private ContentComparatorKPI GetUnitUnderTest()
        {
            return new ContentComparatorKPI(LandingPageGuid);
        }

        [Fact]
        public void Call_Evaluate_ReturnsTrue()
        {
            var kpi = GetUnitUnderTest();
            _content.SetupGet(c => c.ContentGuid).Returns(LandingPageGuid);
            var result = kpi.Evaluate(_content.Object);
            Assert.True(result, "Evaluate should have returned true");
        }

        [Fact]
        public void Call_Evaluate_ReturnsFalse()
        {
            var kpi = GetUnitUnderTest();
            _content.SetupGet(c => c.ContentGuid).Returns(Guid.NewGuid());
            var result = kpi.Evaluate(_content.Object);
            Assert.False(result, "Evaluate should have returned false");
        }

        [Fact]
        public void VerifyGet()
        {
            var kpi = GetUnitUnderTest();
            Assert.True( kpi.ContentGuid.Equals(LandingPageGuid));
        }

        [Fact]
        public void VerifyGetDefaultConstructor()
        {
            var kpi = new ContentComparatorKPI()
            {
                ContentGuid = LandingPageGuid
            };

            Assert.True(kpi.ContentGuid.Equals(LandingPageGuid));
        }
    }
}
