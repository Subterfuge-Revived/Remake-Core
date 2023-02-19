using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Core.Entities.Positions
{
	public class Generator : Outpost
	{
		/// <summary>
		/// Generator constructor
		/// </summary>
		/// <param name="id">ID of the outpost</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		public Generator(string id, RftVector outpostStartPosition, TimeMachine timeMachine) : base(id, outpostStartPosition, timeMachine)
		{
		}

		/// <summary>
		/// Generator constructor
		/// </summary>
		/// <param name="id">ID of the outpost</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		/// <param name="outpostOwner">Owner of outpost</param>
		public Generator(string id, RftVector outpostStartPosition, Player outpostOwner, TimeMachine timeMachine) : base(id, outpostStartPosition, timeMachine, outpostOwner)
		{
		}

		public override OutpostType GetOutpostType()
		{
			return OutpostType.Generator;
		}
	}
}
