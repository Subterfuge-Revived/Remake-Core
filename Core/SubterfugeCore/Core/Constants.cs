namespace Subterfuge.Remake.Core
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
		public const int BaseFactoryProductionAmount = 6;

		/// <summary>
		/// The base time (in minutes) per production
		/// </summary>
		public const int TicksPerProduction = 32;

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
		
		/// <summary>
		/// The amount of units a sub with speed multiplier of 1 travels in an Rft map in 24 hours. 144 units / day = 0.1 units / minute = 1 unit / 10 minutes
		/// By default, subs will move 1 unit per tick.
		/// </summary>
		public const float STANDARD_SUB_RFT_UNITS_PER_DAY = 144;

		/// <summary>
		/// The number of ticks required to elapse before 1 shield is generated
		/// (10 ticks * 20 shields * 15m per tick) / 60 m per hour = 50 hours to regen 20 shields.
		/// </summary>
		public const int BASE_SHIELD_REGENERATION_TICKS = 10;

		/// <summary>
		/// The number of specialists to offer per cycle
		/// </summary>
		public const int SPECIALISTS_OFFERED_PER_CYCLE = 2;
		
		/// <summary>
		/// The number of specialists required in the player's deck when they join a game.
		/// </summary>
		public const int REQUIRED_PLAYER_SPECIALIST_DECK_SIZE = 15;
    }
}
