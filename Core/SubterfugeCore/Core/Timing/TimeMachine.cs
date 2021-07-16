using System.Collections.Generic;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Entities.Managers;
using SubterfugeCore.Core.GameEvents.NaturalGameEvents.vision;
using System;
using SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents;

namespace SubterfugeCore.Core.Timing
{
	public class TimeMachine
	{
		// List of known events
		private ReversePriorityQueue<GameEvent> _pastEventQueue = new ReversePriorityQueue<GameEvent>();
		private PriorityQueue<GameEvent> _futureEventQueue = new PriorityQueue<GameEvent>();

		private VisionManager _visionManager;

		// Current representation of the game state
		private GameState _gameState;

		/// <summary>
		/// Creates a new instance of the TimeMachine. You will likely never need to call this as this is created in the
		/// `Game` object when the game is created.
		/// </summary>
		/// <param name="state">The initial GameState</param>
		public TimeMachine(GameState state)
		{
			_gameState = state;
			_visionManager = new VisionManager(state.GetPlayers());
		}

		/// <summary>
		/// Get the time machine's current state
		/// </summary>
		/// <returns>The GameState at the current time of the TimeMachine</returns>
		public GameState GetState()
		{
			return this._gameState;
		}

		public VisionManager GetVisionManager()
		{
			return _visionManager;
		}

		/// <summary>
		/// Adds an event to the future event queue
		/// </summary>
		/// <param name="gameEvent">The game event to add to the Queue</param>
		public void AddEvent(GameEvent gameEvent)
		{
			this._futureEventQueue.Enqueue(gameEvent);
		}

		/// <summary>
		/// Removes a GameEvent from the game.
		/// </summary>
		/// <param name="gameEvent">The GameEvent to remove from the queue</param>
		public void RemoveEvent(GameEvent gameEvent)
		{
			if (this._futureEventQueue.GetQueue().Contains(gameEvent))
			{
				this._futureEventQueue.Remove(gameEvent);
			}
			else
			{
				// Go to 1 tick before the event occurs.
				GameTick currentTick = GetCurrentTick();
				GoTo(gameEvent);
				Rewind(1);
				this._futureEventQueue.Remove(gameEvent);
				GoTo(currentTick);
			}
		}

		/// <summary>
		/// Advances the game state to the next tick. Creates (or recalculates) vision events for the tick.
		/// </summary>
		public void NextTick()
		{
			this._gameState.CurrentTick = this._gameState.CurrentTick.Advance(1);
			Console.WriteLine("Tick " + this._gameState.CurrentTick.GetTick());
			if (!this._visionManager.DoesRecalculateVisonEventExist(this.GetCurrentTick()))
			{
				RecalculateVisionEvent recalculateVisionEvent = new RecalculateVisionEvent(this.GetCurrentTick());
				AddEvent(recalculateVisionEvent);
				this._visionManager.AddVisionRecalculation(recalculateVisionEvent);
			}
			while (this._futureEventQueue.Count > 0 && this._futureEventQueue.Peek().GetOccursAt() <= this.GetCurrentTick())
			{
				GameEvent futureToPast = this._futureEventQueue.Dequeue();
				Console.WriteLine(futureToPast.GetType().ToString().Substring(futureToPast.GetType().ToString().LastIndexOf('.') + 1));
				futureToPast.ForwardAction(this, _gameState);
				this._pastEventQueue.Enqueue(futureToPast);
			}
		}

		/// <summary>
		/// Reverts the game state to the previous tick. Vision events made in the current (or future) ticks are not altered.
		/// </summary>
		public void PreviousTick()
		{
			while (_pastEventQueue.Count > 0 && _pastEventQueue.Peek().GetOccursAt() >= this.GetCurrentTick())
			{
				// Move commands from the past to the future
				GameEvent pastToFuture = _pastEventQueue.Dequeue();
				pastToFuture.BackwardAction(this, _gameState);
				this._futureEventQueue.Enqueue(pastToFuture);
			}
			this._gameState.CurrentTick = this._gameState.CurrentTick.Rewind(1);
		}

		/// <summary>
		/// Jumps to a specific GameTick. Does not set vision events.
		/// </summary>
		/// <param name="tick">The GameTick to jump to</param>
		public void GoTo(GameTick tick)
		{
			if (tick > this._gameState.CurrentTick)
			{
				Advance(tick - this._gameState.CurrentTick);
			}
			else if (tick < this._gameState.CurrentTick)
			{
				Rewind(this._gameState.CurrentTick - tick);
			}
		}

		/// <summary>
		/// Gets the GameTick that the time machine is currently representing.
		/// </summary>
		/// <returns>The GameTick that the timeMachine is showing</returns>
		public GameTick GetCurrentTick()
		{
			return _gameState.CurrentTick;
		}


		/// <summary>
		/// Jumps to a specific GameEvent
		/// </summary>
		/// <param name="eventOfInterest">The GameEvent to jump to</param>
		public void GoTo(GameEvent eventOfInterest)
		{
			this.GoTo(eventOfInterest.GetOccursAt());
		}

		/// <summary>
		/// For debugging. Advances the timeMachine by a specified number of ticks.
		/// </summary>
		/// <param name="ticks">The number of ticks to advance</param>
		public void Advance(int ticks)
		{
			for (int i = 0; i < ticks; i++)
			{
				NextTick();
			}
		}

		/// <summary>
		/// For debugging. Rewinds the timeMachine by a specified number of ticks.
		/// </summary>
		/// <param name="ticks">The number of ticks to rewind</param>
		public void Rewind(int ticks)
		{
			for (int i = 0; i < ticks; i++)
			{
				PreviousTick();
			}
		}

		/// <summary>
		/// Gets a list of all game events that occur on a given tick.
		/// </summary>
		/// <param name="gameTick">The game tick to get the events of.</param>
		/// <returns>A list of all game evnts that occur on a given tick. Returns an empty list if there are none.</returns>
		public List<GameEvent> GetEventsOnTick(GameTick gameTick)
		{
			if (gameTick > this.GetCurrentTick())
			{
				List<GameEvent> futureEventList = this._futureEventQueue.GetQueue();
				if (futureEventList.Count == 0)
				{
					return new List<GameEvent>();
				}
				// Binary search 
				int start = 0;
				int end = futureEventList.Count - 1;
				while (start < end)
				{
					int mid = (start + end) / 2;
					if (futureEventList[mid].GetOccursAt() < gameTick)
					{
						start = mid + 1;
					}
					else
					{
						end = mid;
					}
				}
				List<GameEvent> returnList = new List<GameEvent>();
				while (start < futureEventList.Count && futureEventList[start].GetOccursAt() == gameTick)
				{
					returnList.Add(futureEventList[start]);
					start++;
				}
				return returnList;
			}
			else
			{
				List<GameEvent> pastEventList = this._pastEventQueue.GetQueue();
				if (pastEventList.Count == 0)
				{
					return new List<GameEvent>();
				}
				// Binary search 
				int start = 0;
				int end = pastEventList.Count - 1;
				while (start < end)
				{
					int mid = (start + end) / 2;
					if (pastEventList[mid].GetOccursAt() > gameTick)
					{
						start = mid + 1;
					}
					else
					{
						end = mid;
					}
				}
				List<GameEvent> returnList = new List<GameEvent>();
				while (end >= 0 && pastEventList[end].GetOccursAt() == gameTick)
				{
					returnList.Add(pastEventList[end]);
					end--;
				}
				return returnList;
			}
		}

		/// <summary>
		/// Gets a list of the queued events
		/// </summary>
		/// <returns>A list of the events in the future event queue</returns>
		public List<GameEvent> GetQueuedEvents()
		{
			List<GameEvent> gameEvents = new List<GameEvent>();
			foreach (GameEvent gameEvent in this._futureEventQueue.GetQueue())
			{
				if (gameEvent != null)
				{
					gameEvents.Add(gameEvent);
				}
			}
			return gameEvents;
		}
	}
}
