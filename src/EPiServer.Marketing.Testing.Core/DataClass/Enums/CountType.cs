namespace EPiServer.Marketing.Testing.Core.DataClass.Enums
{ 
    /// <summary>
    /// IncrementCount call uses this to know which increase.
    /// </summary>
    public enum CountType
    {
        /// <summary>
        /// Represents when a user views the item under test.
        /// </summary>
        View,

        /// <summary>
        /// Represents when a user converts based on the KPI(s) associated with a test.
        /// </summary>
        Conversion
    }
}
