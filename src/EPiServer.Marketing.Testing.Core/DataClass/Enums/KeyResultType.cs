namespace EPiServer.Marketing.Testing.Core.DataClass.Enums
{
    /// <summary>
    /// Type of kpi data to save.
    /// </summary>
    public enum KeyResultType
    {
        /// <summary>
        /// A monetary value.
        /// </summary>
        Financial,

        /// <summary>
        /// Any non monetary amount.
        /// </summary>
        Value,

        /// <summary>
        /// Boolean conversion result used with multiple kpi's.
        /// </summary>
        Conversion
    }
}
