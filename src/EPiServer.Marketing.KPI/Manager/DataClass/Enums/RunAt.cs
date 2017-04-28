namespace EPiServer.Marketing.KPI.Manager.DataClass.Enums
{
    /// <summary>
    /// Used to indicate if the KPI should be evaluated server side or client side.
    /// </summary>
    public enum RunAt
    {
        /// <summary>
        ///  KPI should be run server-side.
        /// </summary>
        Server = 0,

        /// <summary>
        /// KPI should be run client-side.
        /// </summary>
        Client = 1
    }
}
