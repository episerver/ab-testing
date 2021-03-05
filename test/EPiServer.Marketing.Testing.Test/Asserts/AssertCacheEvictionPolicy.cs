using EPiServer.Framework.Cache;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Asserts
{
    [ExcludeFromCodeCoverage]
    public static class AssertCacheEvictionPolicy
    {
        public static bool AreEquivalent(CacheEvictionPolicy expected, CacheEvictionPolicy actual)
        {
            Assert.Equal(expected.Expiration.TotalMinutes, actual.Expiration.TotalMinutes);
            Assert.Equal(expected.TimeoutType, actual.TimeoutType);

            return expected.MasterKeys.Zip(actual.MasterKeys, (e, a) => new { Expected = e, Actual = a })
                            .All(x => AreEqual(x.Expected, x.Actual));
        }

        public static bool AreEqual(string expected, string actual)
        {
            Assert.Equal(expected, actual);

            return true;
        }
    }
}
