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
		/// The base time (in minutes) per production
		/// </summary>
		public const int MINUTES_PER_PRODUCTION = 480;

		/// <summary>
		/// The initial number of drillers an outpost will have when generated.
		/// </summary>
		public const int INITIAL_DRILLERS_PER_OUTPOST = 10;
		
		/// <summary>
		/// The initial number of drillers an outpost will have when generated.
		/// </summary>
		public const int INITIAL_MAX_SHIELDS_PER_OUTPOST = 20;

		/// <summary>
		/// The default vision range for an outpost.
		/// </summary>
		public const float BASE_OUTPOST_VISION_RADIUS = 50;

		/// <summary>
		/// The default vision for a watchtower.
		/// </summary>
		public const float BASE_WATCHTOWER_VISION_RADIUS = 75;

		/// <summary>
		/// The base sub vision radius.
		/// </summary>
		public const float BASE_SUB_VISION_RADIUS = BASE_OUTPOST_VISION_RADIUS * 0.2f;

		/// <summary>
		/// The amount of neptunium required to win.
		/// </summary>
		public const int MINING_NEPTUNIUM_TO_WIN = 200;

		/// <summary>
		/// The driller cost for the first {2} drilled mines.
		/// </summary>
		public static readonly int[] MINING_COST_INITIAL = { 50, 100 };

		/// <summary>
		/// The driller cost increase for each mine beyond the first {2} drilled mines.
		/// </summary>
		public const int MINING_COST_INCREASE = 100;
    }
}
