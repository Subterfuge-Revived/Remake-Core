using System.Collections.Generic;

namespace SubterfugeCore.Core.Players.Currency
{
	public class CurrencyManager
	{
		/// <summary>
		/// A table of all the player's currencies
		/// </summary>
		private readonly Dictionary<CurrencyType, Currency> _currencies = new Dictionary<CurrencyType, Currency>();

		/// <summary>
		/// Creates a new currency as long as one with the same name doesn't already exist. Returns false if failed.
		/// </summary>
		/// <param name="currencyType">The type of currency (specified in Currency.cs CurrencyType enum)</param>
		/// <param name="currencyValue">The inital value of the currency</param>
		/// <param name="canBeNegative">Can the currency go into the negatives</param>
		/// <returns>True if succeeded</returns>
		private bool CreateCurrency(CurrencyType currencyType, int currencyValue, bool canBeNegative = true)
		{
			if (_currencies.ContainsKey(currencyType) == false)
			{
				Currency newCurrency = new Currency(currencyValue, canBeNegative);
				_currencies.Add(currencyType, newCurrency);
				return true; // Return true if the value was successfully created
			}
			return false; // Return false if the value was not created
		}

		/// <summary>
		/// Add a value to a created currency. Returns false if failed. Pass a negative value to subtract.
		/// </summary>
		/// <param name="currencyType">The type of currency (specified in Currency.cs CurrencyType enum)</param>
		/// <param name="addition">The value to add to the currency. Make negative if you wish to subtract.</param>
		/// <returns>True if succeeded</returns>
		public bool AddCurrency(CurrencyType currencyType, int addition)
		{
			if (_currencies.ContainsKey(currencyType))
			{
				bool canGoNegative = _currencies[currencyType].CanBeNegative;
				int oldValue = _currencies[currencyType].Value;
				int newValue = oldValue + addition;
				
				if (newValue < 0)
				{
					if (canGoNegative)
					{
						_currencies[currencyType].Value = newValue;
						return true;
					}
					return false;
				}
				_currencies[currencyType].Value = newValue;
				return true;
			}
			return SetCurrency(currencyType, addition);
		}

		/// <summary>
		/// Directly sets the currency's value. Returns false if failed. Must adhere to the CanBeNegative value.
		/// </summary>
		/// <param name="currencyType">The type of currency (specified in Currency.cs CurrencyType enum)</param>
		/// <param name="newValue">The value of the currency</param>
		/// <param name="canGoNegative">Used when creating a new currency, or altering the bool of another one</param>
		/// <returns>True if succeeded</returns>
		public bool SetCurrency(CurrencyType currencyType, int newValue, bool canGoNegative = true)
		{
			if (_currencies.ContainsKey(currencyType))
			{
				if (newValue < 0)
				{
					if (_currencies[currencyType].CanBeNegative)
					{
						_currencies[currencyType].Value = newValue;
						return true;
					}
					return false;
				}
				_currencies[currencyType].Value = newValue;
				return true; // Successfully set negative value
			}
			CreateCurrency(currencyType, newValue, canGoNegative);
			return true;
		}

		/// <summary>
        /// Used to get the value of a certain currency
        /// </summary>
        /// <param name="currencyType">The type of currency (specified in Currency.cs CurrencyType enum)</param>
        /// <returns> A number when succeeded</returns>  
		public int? GetCurrency(CurrencyType currencyType)
        {
			if (_currencies.ContainsKey(currencyType))
            {
				return _currencies[currencyType].Value;
            }
			return 0;
        }
	}
}
