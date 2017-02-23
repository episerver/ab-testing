namespace EPiServer.Marketing.KPI.Manager.DataClass.Enums
{
    /// <summary>
    /// Comparator used for determining the winning result.
    /// </summary>
    public enum ResultComparison
    {
        /// <summary>
        /// The winning result should be greater than the losing result.
        /// </summary>
        Greater = 0,

        /// <summary>
        /// The winning result should be less than the losing result.
        /// </summary>
        Lesser = 1
    }
}
