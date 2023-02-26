using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Core.Entities.Positions
{
	public class Mine : Outpost
	{
		/// <summary>
		/// Mine constructor
		/// </summary>
		/// <param name="id">THe id of the mine</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		public Mine(
			string id,
			RftVector outpostStartPosition,
			TimeMachine timeMachine
		) : base(id, outpostStartPosition, timeMachine) {
		}

		/// <summary>
		/// Mine constructor
		/// </summary>
		/// <param name="id">The id of the mine</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		/// <param name="outpostOwner">Owner of outpost</param>
		public Mine(
			string id,
			RftVector outpostStartPosition,
			Player outpostOwner,
			TimeMachine timeMachine
		) : base(id, outpostStartPosition, timeMachine, outpostOwner) {
			AddComponent(new MineProductionComponent(this, timeMachine));
		}

		/// <summary>
		/// Mine constructor. Use in preparation to replace a non-mine outpost with a mine outpost with the same properties.
		/// </summary>
		/// <param name="o">The outpost to replicate as a mine</param>
		public Mine(
			Outpost o,
			TimeMachine timeMachine
		) : base(o) {
			AddComponent(new MineProductionComponent(this, timeMachine));
		}

		public override OutpostType GetOutpostType()
		{
			return OutpostType.Mine;
		}
	}
}
