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
		public const int BaseDrillerCapacity = 150;

		/// <summary>
		/// The base capacity of generator type outposts
		/// </summary>
		public const int BaseGeneratorCapacity = 50;

		/// <summary>
		/// The base factory production per tick
		/// </summary>
		public const int BaseFactoryProduction = 6;

		/// <summary>
		/// The base time (in minutes) per production
		/// </summary>
		public const int MinutesPerProduction = 480;

		/// <summary>
		/// The initial number of drillers an outpost will have when generated.
		/// </summary>
		public const int InitialDrillersPerOutpost = 10;
		
		/// <summary>
		/// The initial number of drillers an outpost will have when generated.
		/// </summary>
		public const int InitialMaxShieldsPerOutpost = 20;

		/// <summary>
		/// The default vision range for an outpost.
		/// </summary>
		public const float BaseOutpostVisionRadius = 50;

		/// <summary>
		/// The default vision for a watchtower.
		/// </summary>
		public const float BaseWatchtowerVisionRadius = 75;

		/// <summary>
		/// The base sub vision radius.
		/// </summary>
		public const float BaseSubVisionRadius = BaseOutpostVisionRadius * 0.2f;

		/// <summary>
		/// The amount of neptunium required to win.
		/// </summary>
		public const int MiningNeptuniumToWin = 200;

		/// <summary>
		/// The driller cost for the first {2} drilled mines.
		/// </summary>
		public static readonly int[] MiningCostInitial = { 50, 100 };

		/// <summary>
		/// The driller cost increase for each mine beyond the first {2} drilled mines.
		/// </summary>
		public const int MiningCostIncrease = 100;
    }
}
