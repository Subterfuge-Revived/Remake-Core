using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Entities.Positions
{
	/// <summary>
	/// Factory Class
	/// </summary>
	public class Factory : Outpost
	{
		public static double STANDARD_TICKS_PER_PRODUCTION = Constants.MINUTES_PER_PRODUCTION / GameTick.MINUTES_PER_TICK;

		/// <summary>
		/// Production time in game ticks
		/// </summary>
		private double _ticksPerProduction;

		/// <summary>
		/// Factory Constructor
		/// </summary>
		/// <param name="outpostStartPosition">Position of outpost</param>
		public Factory(string id, RftVector outpostStartPosition) : base(id, outpostStartPosition)
		{
			this._ticksPerProduction = STANDARD_TICKS_PER_PRODUCTION;
		}

		/// <summary>
		/// Factory Constructor
		/// </summary>
		/// <param name="outpostStartPosition">Position of outpost</param>
		/// <param name="outpostOwner">Owner of outpost</param>
		public Factory(string id, RftVector outpostStartPosition, Player outpostOwner) : base(id, outpostStartPosition, outpostOwner)
		{
			this._ticksPerProduction = STANDARD_TICKS_PER_PRODUCTION;
		}
		public override OutpostType GetOutpostType()
		{
			return OutpostType.Factory;
		}

		public override float getVisionRange()
		{
			return Constants.BASE_OUTPOST_VISION_RADIUS;
		}
	}
}