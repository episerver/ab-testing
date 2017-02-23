namespace EPiServer.Marketing.KPI.Manager.DataClass.Enums
{
    /// <summary>
    /// Used to indicate if the kpi should be evaluated server side or client side.
    /// </summary>
    public enum RunAt
    {
        /// <summary>
        ///  Kpi should be run server-side.
        /// </summary>
        Server = 0,

        /// <summary>
        /// Kpi should be run client-side.
        /// </summary>
        Client = 1
    }
}
