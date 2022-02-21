namespace SubterfugeCore.Core.Players.Currency
{
    /// <summary>
	/// A currency object. This is used by the CurrencyManager.
	/// </summary>
    class Currency
    {
        public int Value;
        public readonly bool CanBeNegative;

        /// <summary>
        /// THe currency class constructor
        /// </summary>
        /// <param name="value">The initial currency value</param>
        /// <param name="canBeNegative">If the currency can be negative</param>
        public Currency(int value = 0, bool canBeNegative = true)
        {
            this.Value = value;
            this.CanBeNegative = canBeNegative;
        }
    }
    
    /// <summary>
	/// The enum that lists the currency types.
	/// </summary>
    public enum CurrencyType
    {
        Specialist
    }
}
