using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Players
{
	public class PlayerCurrency
	{
		/// <summary>
		/// A table of all the player's currencies
		/// </summary>
		private Dictionary<String, Dictionary<String,Object>> Currencies = new Dictionary<String, Dictionary<String, Object>>();

		/// <summary>
		/// Creates a new currency as long as one with the same name doesn't already exist. Returns false if failed.
		/// </summary>
		/// <param name="currencyName">The name of the currency</param>
		/// <param name="currencyValue">The inital value of the currency</param>
		/// <param name="canBeNegative">Can the currency go into the negatives</param>
		/// <returns>True if succeeded</returns>
		public bool CreateCurrency(String currencyName, int currencyValue, bool canBeNegative)
		{
			if (Currencies.ContainsKey(currencyName) == false)
			{
				var currencyValues = new Dictionary<String, Object>() {
					["Amount"] = currencyValue,
					["CanBeNegative"] = canBeNegative
				};
				Currencies.Add(currencyName, currencyValues);
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
		/// <param name="currencyName">The name of the currency</param>
		/// <param name="addition">The value to add to the currency. Make negative if you wish to subtract.</param>
		/// <returns>True if succeeded</returns>
		public bool AddCurrency(String currencyName, int addition)
		{
			if (Currencies.ContainsKey(currencyName) == true)
			{
				bool CanGoNegative = (bool)Currencies[currencyName]["CanBeNegative"];
				int OldValue = (int)Currencies[currencyName]["Amount"];
				int NewValue = OldValue + addition;

				if (NewValue < 0)
				{
					if (CanGoNegative == true)
					{
						var newCurrencyValues = new Dictionary<String, Object>()
						{
							["Amount"] = NewValue,
							["CanBeNegative"] = CanGoNegative
						};
						Currencies.Remove(currencyName);
						Currencies.Add(currencyName, newCurrencyValues);
						return true; // Successfully set negative value
					}
					else
					{
						return false; // Return false if the value is negative, but needs to be positive
					}
				}
				else
				{
					var newCurrencyValues = new Dictionary<String, Object>()
					{
						["Amount"] = NewValue,
						["CanBeNegative"] = CanGoNegative
					};
					Currencies.Remove(currencyName);
					Currencies.Add(currencyName, newCurrencyValues);
					return true; // Returns true if the value is set and is positive
				}
			}
			else
			{
				return false; // Return false if no value exists
			}
		}

		/// <summary>
		/// Destroys the given currency, removing it from the system. Returns false if failed.
		/// </summary>
		/// <param name="currencyName">The name of the currency</param>
		/// <returns>True if succeeded</returns>
		public bool DestroyCurrency(String currencyName)
		{
			if (Currencies.ContainsKey(currencyName) == true)
			{
				Currencies.Remove(currencyName);
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Directly sets the currency's value. Returns false if failed. Must adhere to the CanBeNegative value.
		/// </summary>
		/// <param name="currencyName">The name of the currency</param>
		/// <param name="newValue">The value of the currency</param>
		/// <returns>True if succeeded</returns>
		public bool SetCurrency(String currencyName, int newValue)
		{
			if (Currencies.ContainsKey(currencyName) == true)
			{
				bool CanGoNegative = (bool)Currencies[currencyName]["CanBeNegative"];

				if (newValue < 0)
				{
					if (CanGoNegative == true)
					{
						var newCurrencyValues = new Dictionary<String, Object>()
						{
							["Amount"] = newValue,
							["CanBeNegative"] = CanGoNegative
						};
						Currencies.Remove(currencyName);
						Currencies.Add(currencyName, newCurrencyValues);
						return true;
					}
					else
					{
						return false;
					}
				}
				else
				{
					var newCurrencyValues = new Dictionary<String, Object>()
					{
						["Amount"] = newValue,
						["CanBeNegative"] = CanGoNegative
					};
					Currencies.Remove(currencyName);
					Currencies.Add(currencyName, newCurrencyValues);
					return true;
				}
			}
			else
			{
				return false;
			}
		}

		/// <summary>
        /// Used to get the value of a certian currency
        /// </summart>
        /// <param name="currencyName">The name of the currency</param>
        /// <returns> A number when succeeded</returns>  
		public GetCurrency(String currencyName)
        {
			if (Currencies.ContainsKey(currencyName)==true)
            {
				int Value = (int)Currencies[currencyName]["Amount"];
				return Value; // Returns the currency value
            }
			else
            {
				return null; // Return null if no key is found
            }
        }
	}
}
