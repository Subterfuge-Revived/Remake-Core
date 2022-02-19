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
		/// <param name="id">The id of the outpost</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		public Watchtower(string id, RftVector outpostStartPosition) : base(id, outpostStartPosition)
		{
		}

		/// <summary>
		/// Generator constructor
		/// </summary>
		/// <param name="id">THe id of the outpost</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		/// <param name="outpostOwner">Owner of outpost</param>
		public Watchtower(string id, RftVector outpostStartPosition, Player outpostOwner) : base(id, outpostStartPosition, outpostOwner, Constants.BASE_WATCHTOWER_VISION_RADIUS)
		{
		}

		public override OutpostType GetOutpostType()
		{
			return OutpostType.Watchtower;
		}
	}
}
