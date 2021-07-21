using System;
using System.Collections.Generic;
using System.Text;
using GameEventModels;
using Google.Protobuf;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.NaturalGameEvents.outpost;
using SubterfugeCore.Core.GameEvents.Reversible.PlayerTriggeredEvents;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents
{
	public class DrillMineEvent : PlayerTriggeredEvent
	{
		private Outpost _original;
		private Mine _drilledMine;
		
		private DrillMineEventData _drillMineEventData;
        
		public DrillMineEvent(GameEventModel eventModel) : base(eventModel)
		{
		}

		public DrillMineEvent(ITargetable drillLocation, GameTick occursAt) : base(
			Guid.NewGuid().ToString(),
			EventType.DrillMineEvent,
			drillLocation.GetOwner(),
			occursAt,
			new DrillMineEventData()
			{
				SourceId = drillLocation.GetId()
			}.ToByteString()
		) {}

		public override void ForwardAction(TimeMachine timeMachine)
		{
			GameState state = timeMachine.GetState();
			ICombatable combatable = state.GetCombatableById(_drillMineEventData.SourceId);
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
						timeMachine.AddEvent(new NeptuniumProductionEvent(_drilledMine, new GameTick(model.OccursAtTick).Advance(Mine.TICKS_PER_PRODUCTION_PER_MINE / state.GetPlayerOutposts(_drilledMine.GetOwner()).Count)));
					}
				}
			}
		}

		public override void BackwardAction(TimeMachine timeMachine)
		{
			timeMachine.GetState().ReplaceOutpost(_drilledMine, _original);
			_original.GetOwner().AlterMinesDrilled(-1);
			_original.AddDrillers(_original.GetOwner().GetRequiredDrillersToMine());
		}

		public override void parseGameEventModel(GameEventModel eventModel)
		{
			if (eventModel.EventType == EventType.DrillMineEvent)
			{
				_drillMineEventData = DrillMineEventData.Parser.ParseFrom(eventModel.EventData);
			}
		}
	}
}
