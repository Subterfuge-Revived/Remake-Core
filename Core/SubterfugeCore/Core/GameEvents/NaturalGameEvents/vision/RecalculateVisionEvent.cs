using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.NaturalGameEvents.vision
{
	public class RecalculateVisionEvent : GameEvent
	{
		/// <summary>
		/// The tick that the RecalculateVisionEvent evaluates the vision of.
		/// </summary>
		private GameTick _tickOfVisionCalculation;

		private List<IVisionEvent> _visionEvents;

		private string _id;

		public RecalculateVisionEvent(GameTick tick) : base()
		{
			this._tickOfVisionCalculation = tick;
			this._visionEvents = new List<IVisionEvent>();
			this._id = Guid.NewGuid().ToString();
		}

		public override bool ForwardAction(TimeMachine timeMachine, GameState state)
		{
			if (CreateVisionEvents(timeMachine))
			{
				foreach (IVisionEvent visionEvent in this._visionEvents)
				{
					visionEvent.ForwardAction(timeMachine, state);
				}
				List<PlayerTriggeredEvent> triggeredEvents = timeMachine.GetEventsOnTick(this._tickOfVisionCalculation).FindAll(IsPlayerTriggeredEvent).ConvertAll(ConvertToPlayerTriggeredEvent);
				foreach (PlayerTriggeredEvent triggeredEvent in triggeredEvents)
				{
					if (triggeredEvent.WasEventSuccessful())
					{
						triggeredEvent.DetermineVisibility();
					}
				}
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
				foreach (IVisionEvent visionEvent in this._visionEvents)
				{
					visionEvent.BackwardAction(timeMachine, state);
				}
			}
			return EventSuccess;
		}

		public override bool WasEventSuccessful()
		{
			return EventSuccess;
		}

		public override GameTick GetOccursAt()
		{
			return this._tickOfVisionCalculation;
		}

		public override string GetEventId()
		{
			return this._id;
		}

		public override Priority GetPriority()
		{
			return Priority.LOW_PRIORTY;
		}

		private bool IsPlayerTriggeredEvent(GameEvent gameEvent)
		{
			return gameEvent is PlayerTriggeredEvent;
		}

		private PlayerTriggeredEvent ConvertToPlayerTriggeredEvent(GameEvent gameEvent)
		{
			return (PlayerTriggeredEvent)gameEvent;
		}

		/// <summary>
		/// Determines if any Combatables have entered or exited a player's FOV, and if so creates new VisionEvents to represent the new vision state. This method should be called at game initialization and every time the game advances one tick.
		/// </summary>
		/// <param name="state">The state to generate vision events for.</param>
		private bool CreateVisionEvents(TimeMachine tm)
		{
			if (tm.GetCurrentTick() != this._tickOfVisionCalculation)
			{
				return false;
			}
			GameState state = tm.GetState();
			List<ICombatable> combatables = new List<ICombatable>();
			combatables.AddRange(state.GetOutposts());
			combatables.AddRange(state.GetSubList());
			foreach (Player p in state.GetPlayers())
			{
				List<IVision> playerVisionEntities = new List<IVision>();
				playerVisionEntities.AddRange(state.GetPlayerOutposts(p));
				foreach (Sub s in state.GetPlayerSubs(p))
				{
					// Sub could contribute to vision if the sub launch source is not owned by the player or the sub has travelled far enough away from its home outpost
					// TODO: improve algorithm by also checking destination outpost
					if (s.GetSource().GetOwner() != p || s.GetCurrentPosition(state.CurrentTick).Distance(s.GetSource().GetCurrentPosition(state.CurrentTick)) > ((IVision)(s.GetSource())).GetVisionRange() - s.GetVisionRange())
					{
						playerVisionEntities.Add(s);
					}
				}
				foreach (ICombatable combatable in combatables)
				{
					bool isVisible = false;
					foreach (IVision visionEntity in playerVisionEntities)
					{
						if (combatable.GetCurrentPosition(state.CurrentTick).Distance(visionEntity.GetCurrentPosition(state.CurrentTick)) <= visionEntity.GetVisionRange())
						{
							isVisible = true;
							break;
						}
					}
					IVisionEvent visionEvent = null;
					if (isVisible && !combatable.IsVisibleTo(p))
					{
						visionEvent = new EntersVisionEvent(combatable, p, state.CurrentTick);
						Console.WriteLine("Entering Vision Event created on tick " + tm.GetState().GetCurrentTick().GetTick());
					}
					else if (!isVisible && combatable.IsVisibleTo(p))
					{
						visionEvent = new ExitsVisionEvent(combatable, p, state.CurrentTick);
						Console.WriteLine("Exiting Vision Event created on tick " + tm.GetState().GetCurrentTick().GetTick());
					}
					if (visionEvent != null)
					{
						tm.GetVisionManager().AddVisionEvent(visionEvent);
						this._visionEvents.Add(visionEvent);
					}
				}
			}
			return true;
		}

		public List<IVisionEvent> GetVisionEvents()
		{
			return this._visionEvents;
		}
	}
}
