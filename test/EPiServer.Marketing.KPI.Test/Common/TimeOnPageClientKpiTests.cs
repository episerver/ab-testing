using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.KPI.Test.Fakes;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Moq;
using Xunit;

namespace EPiServer.Marketing.KPI.Test.Common
{
    public class TimeOnPageClientKpiTests
    {
        private Mock<IServiceLocator> _serviceLocator;
        private Mock<IContentEvents> _contentEvents;

        private TimeOnPageClientKpi GetUnitUnderTest()
        {
            _serviceLocator = new Mock<IServiceLocator>();
            _serviceLocator.Setup(sl => sl.GetInstance<LocalizationService>())
                .Returns(new FakeLocalizationService("whatever"));
            _contentEvents = new Mock<IContentEvents>();
            _serviceLocator.Setup(sl => sl.GetInstance<IContentEvents>()).Returns(_contentEvents.Object);

            ServiceLocator.SetLocator(_serviceLocator.Object);

            return new TimeOnPageClientKpi();
        }

        [Fact]
        public void TimeOnPageKpi_VerifyGetUIMarkups()
        {
            var kpi = GetUnitUnderTest();

            Assert.NotNull(kpi.UiMarkup);
        }

        [Fact]
        public void TimeOnPageKpi_VerifyGetReadOnlyUIMarkup()
        {
            var kpi = GetUnitUnderTest();
            var data = new Dictionary<string, string>();
            data.Add("TargetDuration", "3");
            kpi.Validate(data);
            Assert.NotNull(kpi.UiReadOnlyMarkup);
        }

        [Fact]
        public void TimeOnPageKpi_ClientEvaluationScript()
        {
            var kpi = GetUnitUnderTest();

            Assert.NotNull(kpi.ClientEvaluationScript);
        }

        [Fact]
        public void TimeOnPageKpi_Validate_TargetDuration_Is_Empty_Throws_Exception()
        {
            var kpi = GetUnitUnderTest();
            var data = new Dictionary<string, string>();
            data.Add("TargetDuration", "");

            Assert.Throws<KpiValidationException>(() => kpi.Validate(data));
        }

        [Fact]
        public void TimeOnPageKpi_Validate_TargetDuration_Is_Invalid_Throws_Exception()
        {
            var kpi = GetUnitUnderTest();
            var data = new Dictionary<string, string>();
            data.Add("TargetDuration", "-1");

            Assert.Throws<KpiValidationException>(() => kpi.Validate(data));
        }

        [Fact]
        public void TimeOnPageKpi_Validate_TargetDuration()
        {
            var kpi = GetUnitUnderTest();
            var data = new Dictionary<string, string>();
            data.Add("TargetDuration", "3");

            kpi.Validate(data);
        }

        [Fact]
        public void TimeOnPageKpi_Evaluate()
        {
            var kpi = GetUnitUnderTest();

            var retVal = kpi.Evaluate(new object(), new EventArgs());

            Assert.IsType<KpiConversionResult>(retVal);
        }
    }
}