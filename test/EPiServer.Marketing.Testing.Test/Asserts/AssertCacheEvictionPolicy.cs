using EPiServer.Framework.Cache;
using System.Linq;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Asserts
{
    public static class AssertCacheEvictionPolicy
    {
        public static bool AreEquivalent(CacheEvictionPolicy expected, CacheEvictionPolicy actual)
        {
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
