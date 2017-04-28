namespace EPiServer.Marketing.KPI.Manager.DataClass
{
    /// <summary>
    /// Used to make sure all client KPIs reference a script.  The script determines when a conversion occurs.
    /// </summary>
    public interface IClientKpi
    {
        /// <summary>
        /// Scripts used to evaluate kpi conversion conditions
        /// </summary>
        string ClientEvaluationScript { get; }
    }
}
