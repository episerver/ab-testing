namespace EPiServer.Marketing.Testing.Dal.EntityModel.Enums
{
    /// <summary>
    /// Type of kpi data to save.
    /// </summary>
    public enum DalKeyResultType
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
