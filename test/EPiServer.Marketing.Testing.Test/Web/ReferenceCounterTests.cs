using EPiServer.Marketing.Testing.Web.Helpers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    [ExcludeFromCodeCoverage]
    public class ReferenceCounterTests
    {
        private ReferenceCounter GetUnitUnderTest()
        {
            return new ReferenceCounter();
        }

        [Fact]
        public void VerifyNoReference()
        {
            var unit = GetUnitUnderTest();
            var hasref = unit.hasReference("bubba");
            Assert.False(hasref, "There should be no reference in a new ReferenceCounter object.");
        }

        [Fact]
        public void VerifyReference()
        {
            var unit = GetUnitUnderTest();
            unit.AddReference("bubba");
            var hasref = unit.hasReference("bubba");
            Assert.True(hasref, "There should be a reference in a new ReferenceCounter object.");
        }

        [Fact]
        public void VerifyRunManyTests()
        {
            var unit = GetUnitUnderTest();
            var iterations = 10000;
            var runManyTests1 = new Thread(
                () =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        for (int x = 0; x < 10; x++)
                        {
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);
                        }
                    }
                }
            );

            var runManyTests2 = new Thread(
                () =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        for (int x = 0; x < 10; x++)
                        {
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                        }
                    }
                }
            );

            var runManyTests3 = new Thread(
                () =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        for (int x = 0; x < 10; x++)
                        {
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);
                            unit.AddReference("bubba" + x);

                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                        }
                    }
                }
            );

            var runManyTests4 = new Thread(
                () =>
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        for (int x = 0; x < 10; x++)
                        {
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                            unit.RemoveReference("bubba" + x);
                        }
                    }
                }
            );

            runManyTests1.Start();
            runManyTests2.Start();
            runManyTests3.Start();
            runManyTests4.Start();

            Assert.True(runManyTests1.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");
            Assert.True(runManyTests2.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");
            Assert.True(runManyTests3.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");
            Assert.True(runManyTests4.Join(TimeSpan.FromSeconds(120)), "The test is taking too long. It's possible that the system has deadlocked.");
        }

        [Fact]
        public void VerifyAllReferenceRemoved()
        {
            var unit = GetUnitUnderTest();
            unit.AddReference("bubba");
            unit.AddReference("bubba");
            unit.AddReference("bubba");
            unit.AddReference("bubba");

            unit.RemoveReference("bubba");
            unit.RemoveReference("bubba");
            unit.RemoveReference("bubba");
            unit.RemoveReference("bubba");

            var hasref = unit.hasReference("bubba");
            Assert.False(hasref, "There should be no reference since all references were removed.");
        }

        [Fact]
        public void VerifyGetReferenceCount()
        {
            var unit = GetUnitUnderTest();

            int count = unit.getReferenceCount("bubba");
            Assert.True((count == 0), "Reference count should be 0.");

            unit.AddReference("bubba");
            unit.AddReference("bubba");
            unit.AddReference("bubba");
            unit.AddReference("bubba");
            count = unit.getReferenceCount("bubba");
            Assert.True((count == 4), "Reference count should be 4.");

            unit.RemoveReference("bubba");
            count = unit.getReferenceCount("bubba");
            Assert.True((count == 3), "Reference count should be 3.");

            unit.RemoveReference("bubba");
            unit.RemoveReference("bubba");
            unit.RemoveReference("bubba");
            count = unit.getReferenceCount("bubba");
            Assert.True((count == 0), "Reference count should be 0.");

            var hasref = unit.hasReference("bubba");
            Assert.False(hasref, "There should be no reference since all references were removed.");
        }
    }
}
