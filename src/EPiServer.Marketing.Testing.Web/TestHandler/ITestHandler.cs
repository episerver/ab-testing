using System;

namespace EPiServer.Marketing.Testing.Web
{
    /// <summary>
    /// The test handler interface defines methods that handle various cms events.  
    /// </summary>
    public interface ITestHandler
    {
        /// <summary>
        /// Swaps content, increments views, manages test cookies as needed.  
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LoadedContent(object sender, ContentEventArgs e);

        /// <summary>
        /// Proxy for Kpi evaluation. Increments conversions, manages test cookies as needed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ProxyEventHandler(object sender, EventArgs e);

        /// <summary>
        /// Enables AB Testing
        /// </summary>
        void EnableABTesting();

        /// <summary>
        /// Disables AB Testing
        /// </summary>
        void DisableABTesting();
    }
}
