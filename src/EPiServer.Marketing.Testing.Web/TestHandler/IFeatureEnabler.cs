using EPiServer.Marketing.Testing.Core.Manager;

namespace EPiServer.Marketing.Testing.Web
{
    /// <summary>
    /// Defines methods to enable and disable AB testing based on number of active tests in cache.
    /// </summary>
    public interface IFeatureEnabler
    {
        /// <summary>
        /// Enables AbTesting when the active tests count is one.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TestAddedToCache(object sender, TestEventArgs e);

        /// <summary>
        /// Disables AbTesting when the active tests count is zero.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TestRemovedFromCache(object sender, TestEventArgs e);
    }
}
