namespace SubterfugeCore.Core.Config
{
    /// <summary>
    /// A Collection of Constants like player base stats etc.
    /// </summary>
    public static class Constants
    {
		/// <summary>
		/// The player's base driller capacity
		/// </summary>
		public const int BASE_DRILLER_CAPACITY = 150;

		/// <summary>
		/// The base capacity of generator type outposts
		/// </summary>
		public const int BASE_GENERATOR_CAPACITY = 50;

		/// <summary>
		/// The base factory production per tick
		/// </summary>
		public const int BASE_FACTORY_PRODUCTION = 6;

		/// <summary>
		/// The base amount of neptunium to produce per tick.
		/// </summary>
		public const int BASE_NEPTUNIUM_PRODUCTION = 1;

		/// <summary>
		/// Amount of neptunium required to win
		/// </summary>
		public const int MINING_NEPTUNIUM_REQUIRED_TO_WIN = 200;

		/// <summary>
		/// The number of outposts a player has to own to win in domination mode.
		/// </summary>
		public const int DOMINATION_OUTPOSTS_REQUIRED_TO_WIN = 40;

    }

}
