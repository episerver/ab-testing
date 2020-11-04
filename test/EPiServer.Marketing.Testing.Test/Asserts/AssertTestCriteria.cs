using EPiServer.Marketing.Testing.Core.DataClass;
using System.Linq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Asserts
{
    public static class AssertTestCriteria
    {
        public static bool AreEquivalent(TestCriteria expected, TestCriteria actual)
        {
            return expected.GetFilters().Zip(actual.GetFilters(), (e, a) => new { Expected = e, Actual = a })
                           .All(x => AreEqual(x.Expected, x.Actual));
        }

        public static bool AreEqual(ABTestFilter expected, ABTestFilter actual)
        {
            Assert.Equal(expected.Operator, actual.Operator);
            Assert.Equal(expected.Property, actual.Property);
            Assert.Equal(expected.Value, actual.Value);

            return true;
        }
    }
}
