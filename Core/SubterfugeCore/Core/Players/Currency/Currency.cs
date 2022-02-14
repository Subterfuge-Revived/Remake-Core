namespace SubterfugeCore.Core.Players.Currency
{
    /// <summary>
	/// A currency object. This is used by the CurrencyManager.
	/// </summary>
    class Currency
    {
        public int value = 0;
        public bool? canBeNegative = true;

        /// <summary>
		/// Currency class constructor. Creates a new currency object with specified parameters.
		/// </summary>
		/// <param name="CValue">The inital value of the currency</param>
		/// <param name="CcanBeNegative">The value that determines if the currency can be negative or not</param>
        public Currency(int Cvalue, bool? CcanBeNegative)
        {
            value = Cvalue;
            canBeNegative = CcanBeNegative;
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
