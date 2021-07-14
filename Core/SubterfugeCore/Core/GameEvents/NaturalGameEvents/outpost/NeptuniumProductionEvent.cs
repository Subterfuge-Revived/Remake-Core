using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.NaturalGameEvents.outpost
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

		public NeptuniumProductionEvent(Mine mine, GameTick occursAt) : base(occursAt, Priority.NATURAL_PRIORITY_1)
		{
			this._mine = mine;
			this._nextProduction = null;
		}

		public override bool ForwardAction(TimeMachine timeMachine, GameState state)
		{
			if (!_mine.GetOwner().IsEliminated() && state.GetOutposts().Contains(_mine) && !_mine.IsDestroyed())
			{
				_mine.GetOwner().AlterNeptunium(1);
				this._nextProduction = new NeptuniumProductionEvent(_mine, GetOccursAt().Advance(Mine.TICKS_PER_PRODUCTION_PER_MINE / state.GetPlayerOutposts(_mine.GetOwner()).Count));
				timeMachine.AddEvent(this._nextProduction);
				EventSuccess = true;
			}
			else
			{
				EventSuccess = false;
			}
			return EventSuccess;
		}

		public override bool BackwardAction(TimeMachine timeMachine, GameState state)
		{
			if (EventSuccess)
			{
				_mine.GetOwner().AlterNeptunium(-1);
			}
			return EventSuccess;
		}

		public override bool WasEventSuccessful()
		{
			return EventSuccess;
		}
	}
}
