using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.outpost
{
	public class NeptuniumProductionEvent : NaturalGameEvent
	{
		/// <summary>
		/// The mine that is producing neptunium
		/// </summary>
		private Mine _mine;

		/// <summary>
		/// The reference to the next neptunium production event. Is null if it has not been created yet.
		/// </summary>
		private NeptuniumProductionEvent _nextProduction;

		public NeptuniumProductionEvent(Mine mine, GameTick occursAt) : base(occursAt, Priority.LowPriorty)
		{
			this._mine = mine;
			this._nextProduction = null;
		}

		public override bool ForwardAction(TimeMachine timeMachine, GameState.GameState state)
		{
			if (!_mine.GetComponent<DrillerCarrier>().GetOwner().IsEliminated() && state.GetOutposts().Contains(_mine) && !_mine.GetComponent<DrillerCarrier>().IsDestroyed())
			{
				_mine.GetComponent<DrillerCarrier>().GetOwner().AlterNeptunium(1);
				this._nextProduction = new NeptuniumProductionEvent(_mine, GetOccursAt().Advance(Mine.TICKS_PER_PRODUCTION_PER_MINE / state.GetPlayerOutposts(_mine.GetComponent<DrillerCarrier>().GetOwner()).Count));
				timeMachine.AddEvent(this._nextProduction);
				EventSuccess = true;
			}
			else
			{
				EventSuccess = false;
			}
			return EventSuccess;
		}

		public override bool BackwardAction(TimeMachine timeMachine, GameState.GameState state)
		{
			if (EventSuccess)
			{
				_mine.GetComponent<DrillerCarrier>().GetOwner().AlterNeptunium(-1);
			}
			return EventSuccess;
		}

		public override bool WasEventSuccessful()
		{
			return EventSuccess;
		}
	}
}
