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
		public const int INITIAL_DRILLERS_PER_OUTPOST = 30;

		/// <summary>
		/// The default vision range for an outpost.
		/// </summary>
		public const float BASE_OUTPOST_VISION_RADIUS = 50;

		/// <summary>
		/// The default vision for a watchtower.
		/// </summary>
		public const float BASE_WATCHTOWER_VISION_RADIUS = 75;

		/// <summary>
		/// The base sub vision radius
		/// </summary>
		public const float BASE_SUB_VISION_RADIUS = BASE_OUTPOST_VISION_RADIUS * 0.2f;

    }

}
