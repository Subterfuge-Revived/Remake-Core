using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SubterfugeCore.Core.Players.Currency
{
	public class CurrencyManager
	{
		/// <summary>
		/// A table of all the player's currencies
		/// </summary>
		private Dictionary<CurrencyType, Currency> Currencies = new Dictionary<CurrencyType, Currency>();

		/// <summary>
		/// Creates a new currency as long as one with the same name doesn't already exist. Returns false if failed.
		/// </summary>
		/// <param name="currencyType">The type of currency (specified in Currency.cs CurrencyType enum)</param>
		/// <param name="currencyValue">The inital value of the currency</param>
		/// <param name="canBeNegative">Can the currency go into the negatives</param>
		/// <returns>True if succeeded</returns>
		private bool CreateCurrency(CurrencyType currencyType, int currencyValue, bool? canBeNegative)
		{
			if (Currencies.ContainsKey(currencyType) == false)
			{
				Currency newCurrency = new Currency(currencyValue, canBeNegative);
				Currencies.Add(currencyType, newCurrency);
				return true; // Return true if the value was successfully created
			}
			else
			{
				return false; // Return false if the value was not created
			}
		}

		/// <summary>
		/// Add a value to a created currency. Returns false if failed. Pass a negative value to subtract.
		/// </summary>
		/// <param name="currencyType">The type of currency (specified in Currency.cs CurrencyType enum)</param>
		/// <param name="addition">The value to add to the currency. Make negative if you wish to subtract.</param>
		/// <returns>True if succeeded</returns>
		public bool AddCurrency(CurrencyType currencyType, int addition)
		{
			if (Currencies.ContainsKey(currencyType) == true)
			{
				bool CanGoNegative = (bool)Currencies[currencyType].canBeNegative;
				int OldValue = (int)Currencies[currencyType].value;
				int NewValue = OldValue + addition;

				if (NewValue < 0)
				{
					if (CanGoNegative == true)
					{
						Currencies[currencyType].value = NewValue;
						return true; // Successfully set negative value
					}
					else
					{
						return false; // Return false if the value is negative, but needs to be positive
					}
				}
				else
				{
					Currencies[currencyType].value = NewValue;
					return true; // Successfully set negative value
				}
			}
			else
			{
				bool returnedCurrency = SetCurrency(currencyType, addition);
				return returnedCurrency;
			}
		}

		/// <summary>
		/// Directly sets the currency's value. Returns false if failed. Must adhere to the CanBeNegative value.
		/// </summary>
		/// <param name="currencyType">The type of currency (specified in Currency.cs CurrencyType enum)</param>
		/// <param name="newValue">The value of the currency</param>
		/// <param name="canGoNegative">Used when creating a new currency, or altering the bool of another one</param>
		/// <returns>True if succeeded</returns>
		public bool SetCurrency(CurrencyType currencyType, int newValue, [Optional] bool? canGoNegative)
		{
			if (Currencies.ContainsKey(currencyType) == true)
			{
				if (canGoNegative == (bool?)Currencies[currencyType].canBeNegative) {
					canGoNegative = (bool?)Currencies[currencyType].canBeNegative;
                }

				if (newValue < 0)
				{
					if (canGoNegative == true)
					{
						Currencies[currencyType].value = newValue;
						return true; // Successfully set negative value
					}
					else
					{
						return false;
					}
				}
				else
				{
					Currencies[currencyType].value = newValue;
					return true; // Successfully set negative value
				}
			}
			else
			{
				if (canGoNegative == null){
					canGoNegative = true;
				}
				CreateCurrency(currencyType, newValue, canGoNegative);
				return true;
			}
		}

		/// <summary>
        /// Used to get the value of a certian currency
        /// </summary>
        /// <param name="currencyType">The type of currency (specified in Currency.cs CurrencyType enum)</param>
        /// <returns> A number when succeeded</returns>  
		public int? GetCurrency(CurrencyType currencyType)
        {
			if (Currencies.ContainsKey(currencyType)==true)
            {
				int Value = (int)Currencies[currencyType].value;
				return Value; // Returns the currency value
            }
			else
            {
				return null; // Return null if no key is found
            }
        }
	}
}
