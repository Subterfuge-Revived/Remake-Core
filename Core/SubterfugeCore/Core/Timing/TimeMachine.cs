using System.Collections.Generic;
using Subterfuge.Remake.Core.GameEvents.Base;

namespace Subterfuge.Remake.Core.Timing
{
    public class TimeMachine
    {
        // List of known events
        private ReversePriorityQueue<GameEvent> _pastEventQueue = new ReversePriorityQueue<GameEvent>();
        private PriorityQueue<GameEvent> _futureEventQueue = new PriorityQueue<GameEvent>();

        // Current representation of the game state
        private GameState.GameState _gameState;

        /// <summary>
        /// Creates a new instance of the TimeMachine. You will likely never need to call this as this is created in the
        /// `Game` object when the game is created.
        /// </summary>
        /// <param name="state">The initial GameState</param>
        public TimeMachine(GameState.GameState state)
        {
            _gameState = state;
        }

        /// <summary>
        /// Get the time machine's current state
        /// </summary>
        /// <returns>The GameState at the current time of the TimeMachine</returns>
        public GameState.GameState GetState()
        {
            return this._gameState;
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
        /// Jumps to a specific GameTick
        /// </summary>
        /// <param name="tick">The GameTick to jump to</param>
        public void GoTo(GameTick tick)
        {
            if (tick > _gameState.CurrentTick)
            {
                bool evaluating = true;
                while (evaluating)
                {

                    if (_futureEventQueue.Count > 0)
                    {
                        if (_futureEventQueue.Peek().GetOccursAt() <= tick)
                        {
                            // Move commands from the future to the past
                            GameEvent futureToPast = _futureEventQueue.Dequeue();
                            futureToPast.ForwardAction(this, _gameState);
                            _pastEventQueue.Enqueue(futureToPast);
                            continue;
                        }
                    }
                    evaluating = false;
                }
            }
            else
            {
                bool evaluating = true;
                while (evaluating)
                {
                    if (_pastEventQueue.Count > 0)
                    {
                        if (_pastEventQueue.Peek().GetOccursAt() >= tick)
                        {
                            // Move commands from the past to the future
                            GameEvent pastToFuture = _pastEventQueue.Dequeue();
                            pastToFuture.BackwardAction(this, _gameState);
                            _futureEventQueue.Enqueue(pastToFuture);
                            continue;
                        }
                    }
                    evaluating = false;
                }
            }
            this._gameState.CurrentTick = tick;
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
            GameTick tick = this.GetCurrentTick().Advance(ticks);
            this.GoTo(tick);
        }

        /// <summary>
        /// For debugging. Rewinds the timeMachine by a specified number of ticks.
        /// </summary>
        /// <param name="ticks">The number of ticks to rewind</param>
        public void Rewind(int ticks)
        {
            GameTick tick = this.GetCurrentTick().Rewind(ticks);
            this.GoTo(tick);
        }

        /// <summary>
        /// Gets a list of the queued events
        /// </summary>
        /// <returns>A list of the events in the future event queue</returns>
        public List<GameEvent> GetQueuedEvents()
        {
            List<GameEvent> gameEvents = new List<GameEvent>();
            foreach(GameEvent gameEvent in this._futureEventQueue.GetQueue()){
                if (gameEvent != null){
                    gameEvents.Add(gameEvent);
                }
            }
            return gameEvents;
        }
        
        /// <summary>
        /// Get past events
        /// </summary>
        /// <returns>A list of the events in the future event queue</returns>
        public List<GameEvent> GetPastEvents()
        {
            List<GameEvent> gameEvents = new List<GameEvent>();
            foreach(GameEvent gameEvent in this._pastEventQueue.GetQueue()){
                if (gameEvent != null){
                    gameEvents.Add(gameEvent);
                }
            }
            return gameEvents;
        }
    }
}
