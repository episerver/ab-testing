using EPiServer.Marketing.Testing.Web.Helpers;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
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
