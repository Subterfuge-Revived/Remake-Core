using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Entities.Positions
{
	public class Mine : Outpost
	{
		/// <summary>
		/// The rate at which mines produce neptunium. Rate is proportional to number of outposts owned.
		/// </summary>
		public static int TICKS_PER_PRODUCTION_PER_MINE = (int)(1440 / GameTick.MINUTES_PER_TICK); // One neptunium per day (1440 minutes) per outpost

		/// <summary>
		/// Mine constructor
		/// </summary>
		/// <param name="outpostStartPosition">Position of outpost</param>
		public Mine(string id, RftVector outpostStartPosition) : base(id, outpostStartPosition)
		{
		}

		/// <summary>
		/// Mine constructor
		/// </summary>
		/// <param name="outpostStartPosition">Position of outpost</param>
		/// <param name="outpostOwner">Owner of outpost</param>
		public Mine(string id, RftVector outpostStartPosition, Player outpostOwner) : base(id, outpostStartPosition, outpostOwner)
		{
		}

		/// <summary>
		/// Mine constructor. Use in preparation to replace a non-mine outpost with a mine outpost with the same properties.
		/// </summary>
		/// <param name="o">The outpost to replicate as a mine</param>
		public Mine(Outpost o) : base(o)
		{
		}

		public override OutpostType GetOutpostType()
		{
			return OutpostType.Mine;
		}

		public override float GetVisionRange()
		{
			return Constants.BASE_OUTPOST_VISION_RADIUS;
		}
	}
}
