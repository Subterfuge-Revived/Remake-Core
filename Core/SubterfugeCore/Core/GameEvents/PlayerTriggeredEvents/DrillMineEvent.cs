using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents
{
	public class DrillMineEvent : PlayerTriggeredEvent
	{
		public Outpost OriginalOutpost;
		public Mine CreatedMine;

		public DrillMineEvent(GameRoomEvent mining) : base(mining)
		{
		}

		public DrillMineEventData GetEventData()
		{
			return JsonConvert.DeserializeObject<DrillMineEventData>(Model.GameEventData.SerializedEventData);
		}

		public override bool ForwardAction(TimeMachine timeMachine)
		{
			var state = timeMachine.GetState();
			Entity drillLocation = state.GetEntity(GetEventData().SourceId);
			if (drillLocation != null && drillLocation is Outpost && !(drillLocation is Mine) && !((Outpost)drillLocation).GetComponent<DrillerCarrier>().IsDestroyed())
			{
				OriginalOutpost = (Outpost)drillLocation;
				var drillerCarrier = drillLocation.GetComponent<DrillerCarrier>();
				if (state.GetOutposts().Contains(OriginalOutpost) && !drillerCarrier.GetOwner().IsEliminated() && drillerCarrier.GetDrillerCount() >= drillerCarrier.GetOwner().GetRequiredDrillersToMine())
				{
					CreatedMine = new Mine(OriginalOutpost, timeMachine);
					if (state.ReplaceOutpost(OriginalOutpost, CreatedMine))
					{
						drillerCarrier.AlterDrillers(drillerCarrier.GetOwner().GetRequiredDrillersToMine() * -1);
						drillerCarrier.GetOwner().AlterMinesDrilled(1);
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

		public override bool BackwardAction(TimeMachine timeMachine)
		{
			if (EventSuccess)
			{
				var drillerCarrier = CreatedMine.GetComponent<DrillerCarrier>();
				timeMachine.GetState().ReplaceOutpost(CreatedMine, OriginalOutpost);
				drillerCarrier.GetOwner().AlterMinesDrilled(-1);
				drillerCarrier.AlterDrillers(drillerCarrier.GetOwner().GetRequiredDrillersToMine());
			}
			return EventSuccess;
		}

		public override bool WasEventSuccessful()
		{
			return EventSuccess;
		}
	}
}
