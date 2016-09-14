using EPiServer.DataAbstraction;
using EPiServer.Framework.Initialization;
using EPiServer.Marketing.Testing.Data;
using EPiServer.Marketing.Testing.Web.Jobs;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class JobReschedulerTests
    {
        Mock<IServiceLocator> _locator = new Mock<IServiceLocator>();
        Mock<IScheduledJobRepository> _jobRepo = new Mock<IScheduledJobRepository>();

        private JobRescheduler GetUnitUnderTest()
        {
            _locator.Setup(sl => sl.GetInstance<IScheduledJobRepository>()).Returns(_jobRepo.Object);
            _jobRepo.Setup(g => g.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new ScheduledJob(
                            Guid.Empty, "TestSchedulingJob", true,
                            DateTime.MinValue, DateTime.MaxValue, 
                            DateTime.MaxValue.ToUniversalTime(),
                            false, "", new ScheduledIntervalType(), 1, "Execute",
                            false, "TestSchedulingJob", "EPiServer.Marketing.Testing.Web.Jobs",
                             null
                ));

            var unit = new JobRescheduler(_locator.Object);
            return unit;
        }

        [Fact]
        public void VerifyOnTestSaved()
        {
            var list = new List<IMarketingTest>()
            {
                new ABTest() { Id = Guid.NewGuid(),
                    StartDate = DateTime.Now.AddHours(-1),
                    State = Data.Enums.TestState.Inactive,
                    ZScore = 2.4,
                    ConfidenceLevel = 95,
                    Variants = new List<Variant>() {new Variant() {Views = 100, Conversions = 50}, new Variant() {Views = 70, Conversions = 60} }
                }
            };

            var unit = GetUnitUnderTest();
            unit.OnTestSaved(this, new TestEventArgs(list.ToArray()[0]));

            _jobRepo.Verify(sa => sa.Save(It.IsAny<ScheduledJob>()), Times.Once, "Failed to save update job with update time");
        }
    }
}
