using System;
using System.Collections.Generic;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Core.Entities.Positions
{
	/// <summary>
	/// Factory Class
	/// </summary>
	public class Factory : Outpost
	{

		/// <summary>
		/// Factory Constructor
		/// </summary>
		/// <param name="id">ID of the outpost</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		public Factory(string id, RftVector outpostStartPosition, TimeMachine timeMachine) : base(id, outpostStartPosition, timeMachine)
		{
			AddComponent(new DrillerProducer(this, timeMachine));
		}

		/// <summary>
		/// Factory Constructor
		/// </summary>
		/// <param name="id">ID of the outpost</param>
		/// <param name="outpostStartPosition">Position of outpost</param>
		/// <param name="outpostOwner">Owner of outpost</param>
		public Factory(string id, RftVector outpostStartPosition, Player outpostOwner, TimeMachine timeMachine) : base(id, outpostStartPosition, timeMachine, outpostOwner)
		{
			AddComponent(new DrillerProducer(this, timeMachine));
		}
		
		public override OutpostType GetOutpostType()
		{
			return OutpostType.Factory;
		}
	}
}