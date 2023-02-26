using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Core.Entities.Positions
{
	public class Watchtower : Outpost
	{
		/// <summary>
		/// Generator constructor
		/// </summary>
		/// <param name="id">The id of the outpost</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		public Watchtower(
			string id,
			RftVector outpostStartPosition,
			TimeMachine timeMachine
		) : base(id, outpostStartPosition, timeMachine) {
		}

		/// <summary>
		/// Generator constructor
		/// </summary>
		/// <param name="id">THe id of the outpost</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		/// <param name="outpostOwner">Owner of outpost</param>
		public Watchtower(
			string id,
			RftVector outpostStartPosition,
			Player outpostOwner,
			TimeMachine timeMachine
		) : base(id, outpostStartPosition, timeMachine, outpostOwner, Constants.BaseWatchtowerVisionRadius) {
		}

		public override OutpostType GetOutpostType()
		{
			return OutpostType.Watchtower;
		}
	}
}
