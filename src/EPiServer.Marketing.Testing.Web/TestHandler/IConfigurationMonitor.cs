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
    }
}
