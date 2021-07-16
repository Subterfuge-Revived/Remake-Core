using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.NaturalGameEvents.vision
{
	public class ExitsVisionEvent : IVisionEvent
	{
		public ExitsVisionEvent(ICombatable combatable, Player visibleTo, GameTick tick) : base()
		{
			this._entity = combatable;
			this._player = visibleTo;
			this._occursAt = tick;
		}

		public override bool ForwardAction(TimeMachine timeMachine, GameState state)
		{
			if (this._entity.IsVisibleTo(this._player) && state.CurrentTick.Equals(this._occursAt))
			{
				this._entity.SetVisibleTo(this._player, false);
				this.EventSuccess = true;
			}
			else
			{
				this.EventSuccess = false;
			}
			return this.EventSuccess;
		}

		public override bool BackwardAction(TimeMachine timeMachine, GameState state)
		{
			if (this.EventSuccess)
			{
				this._entity.SetVisibleTo(this._player, true);
			}
			return this.EventSuccess;
		}

		public override bool WasEventSuccessful()
		{
			return this.EventSuccess;
		}
	}
}
