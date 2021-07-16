using System;
using System.Collections.Generic;
using System.Text;
using GameEventModels;
using Google.Protobuf;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.GameEvents.NaturalGameEvents.outpost;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents
{
	public class DrillMineEvent : PlayerTriggeredEvent
	{
		private Outpost _original;
		private Mine _drilledMine;

		public DrillMineEvent(GameEventModel miningData) : base(miningData)
		{
		}

		public DrillMineEventData GetEventData()
		{
			return DrillMineEventData.Parser.ParseFrom(model.EventData);
		}

		public override bool ForwardAction(TimeMachine timeMachine, GameState state)
		{
			ICombatable combatable = state.GetCombatableById(GetEventData().SourceId);
			if (combatable != null && combatable is Outpost && !(combatable is Mine) && !((Outpost)combatable).IsDestroyed())
			{
				_original = (Outpost)combatable;
				if (state.GetOutposts().Contains(_original) && !_original.GetOwner().IsEliminated() && _original.GetDrillerCount() >= _original.GetOwner().GetRequiredDrillersToMine())
				{
					_drilledMine = new Mine(_original);
					if (state.ReplaceOutpost(_original, _drilledMine))
					{
						_drilledMine.RemoveDrillers(_original.GetOwner().GetRequiredDrillersToMine());
						_original.GetOwner().AlterMinesDrilled(1);
						timeMachine.AddEvent(new NeptuniumProductionEvent(_drilledMine, GetOccursAt().Advance(Mine.TICKS_PER_PRODUCTION_PER_MINE / state.GetPlayerOutposts(_drilledMine.GetOwner()).Count)));
						EventSuccess = true;
					}
				}
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
				state.ReplaceOutpost(_drilledMine, _original);
				_original.GetOwner().AlterMinesDrilled(-1);
				_original.AddDrillers(_original.GetOwner().GetRequiredDrillersToMine());
			}
			return EventSuccess;
		}

		public override GameEventModel ToGameEventModel()
		{
			GameEventModel baseModel = GetBaseGameEventModel();
			baseModel.EventData = GetEventData().ToByteString();
			return baseModel;
		}

		public override bool WasEventSuccessful()
		{
			return EventSuccess;
		}

		/// <summary>
		/// Returns a new list of players who can see the DrillMineEvent. Throws a null reference exception if the event was not successful.
		/// </summary>
		public override void DetermineVisibility()
		{
			if (_drilledMine is null)
			{
				throw new NullReferenceException();
			}
			this.VisibleTo = new List<Player>(_drilledMine.GetVisibleTo());
		}

		public override Priority GetPriority()
		{
			return Priority.NATURAL_PRIORITY_9;
		}
	}
}
