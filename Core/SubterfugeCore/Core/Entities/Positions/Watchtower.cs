using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Entities.Positions
{
	public class Watchtower : Outpost
	{
		/// <summary>
		/// Generator constructor
		/// </summary>
		/// <param name="outpostStartPosition">Position of outpost</param>
		public Watchtower(string id, RftVector outpostStartPosition) : base(id, outpostStartPosition)
		{
		}

		/// <summary>
		/// Generator constructor
		/// </summary>
		/// <param name="outpostStartPosition">Position of outpost</param>
		/// <param name="outpostOwner">Owner of outpost</param>
		public Watchtower(string id, RftVector outpostStartPosition, Player outpostOwner) : base(id, outpostStartPosition, outpostOwner)
		{
		}

		public override OutpostType GetOutpostType()
		{
			return OutpostType.Watchtower;
		}

		public override float GetVisionRange()
		{
			return Constants.BASE_OUTPOST_VISION_RADIUS * Constants.WATCHTOWER_VISION_RADIUS_MULTIPLIER;
		}
	}
}
