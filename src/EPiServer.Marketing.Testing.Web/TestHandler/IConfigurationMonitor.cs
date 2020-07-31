namespace EPiServer.Marketing.Testing.Web
{
    /// <summary>
    /// Defines methods to monitor configuration changes and apply them to all the remote nodes.
    /// </summary>
    public interface IConfigurationMonitor
    {
        /// <summary>
        /// Enables or disables AB testing based on config changes.
        /// </summary>
        void HandleConfigurationChange();

        /// <summary>
        /// Called by the UI to reset the monitor and force all other nodes to re-read the config.
        /// </summary>
        void Reset();
    }
}
