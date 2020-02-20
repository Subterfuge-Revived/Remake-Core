using System.Collections.Generic;
using SubterfugeCore.Core.GameEvents.Base;

namespace SubterfugeCore.Core.Timing
{
    public class TimeMachine
    {
        // List of known events
        private ReversePriorityQueue<GameEvent> pastEventQueue = new ReversePriorityQueue<GameEvent>();
        private PriorityQueue<GameEvent> futureEventQueue = new PriorityQueue<GameEvent>();

        // Current representation of the game state
        private GameState gameState;

        // To determine the current position of the time machine.
        public GameTick currentTick;
        private GameTick startTime;

        /// <summary>
        /// Creates a new instance of the TimeMachine. You will likely never need to call this as this is created in the
        /// `Game` object when the game is created.
        /// </summary>
        /// <param name="state">The initial GameState</param>
        public TimeMachine(GameState state)
        {
            startTime = state.getStartTick();
            currentTick = state.getCurrentTick();
            gameState = state;
        }

        /// <summary>
        /// Get the time machine's current state
        /// </summary>
        /// <returns>The GameState at the current time of the TimeMachine</returns>
        public GameState getState()
        {
            return this.gameState;
        }

        /// <summary>
        /// Adds an event to the future event queue
        /// </summary>
        /// <param name="gameEvent">The game event to add to the Queue</param>
        public void addEvent(GameEvent gameEvent)
        {
            this.futureEventQueue.Enqueue(gameEvent);
        }
        
        /// <summary>
        /// Removes a GameEvent from the future event queue
        /// </summary>
        /// <param name="gameEvent">The GameEvent to remove from the queue</param>
        public void removeEvent(GameEvent gameEvent)
        {
            this.futureEventQueue.remove(gameEvent);
        }

        /// <summary>
        /// Jumps to a specific GameTick
        /// </summary>
        /// <param name="tick">The GameTick to jump to</param>
        public void goTo(GameTick tick)
        {
            if (tick > currentTick)
            {
                bool evaluating = true;
                while (evaluating)
                {

                    if (futureEventQueue.Count > 0)
                    {
                        if (futureEventQueue.Peek().getTick() < tick)
                        {
                            // Move commands from the future to the past
                            GameEvent futureToPast = futureEventQueue.Dequeue();
                            futureToPast.eventForwardAction();
                            pastEventQueue.Enqueue(futureToPast);
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
                    if (pastEventQueue.Count > 0)
                    {
                        if (pastEventQueue.Peek().getTick() > tick)
                        {
                            // Move commands from the past to the future
                            GameEvent pastToFuture = pastEventQueue.Dequeue();
                            pastToFuture.eventBackwardAction();
                            futureEventQueue.Enqueue(pastToFuture);
                            continue;
                        }
                    }
                    evaluating = false;
                }
            }
            this.currentTick = tick;
            this.gameState.currentTick = tick;
        }

        /// <summary>
        /// Gets the GameTick that the time machine is currently representing.
        /// </summary>
        /// <returns>The GameTick that the timeMachine is showing</returns>
        public GameTick getCurrentTick()
        {
            return this.currentTick;
        }

        
        /// <summary>
        /// Jumps to a specific GameEvent
        /// </summary>
        /// <param name="eventOfInterest">The GameEvent to jump to</param>
        public void goTo(GameEvent eventOfInterest)
        {
            this.goTo(eventOfInterest.getTick());
        }
        
        /// <summary>
        /// For debugging. Advances the timeMachine by a specified number of ticks.
        /// </summary>
        /// <param name="ticks">The number of ticks to advance</param>
        public void advance(int ticks)
        {
            GameTick tick = this.getCurrentTick().advance(ticks);
            this.goTo(tick);
        }

        /// <summary>
        /// For debugging. Rewinds the timeMachine by a specified number of ticks.
        /// </summary>
        /// <param name="ticks">The number of ticks to rewind</param>
        public void rewind(int ticks)
        {
            GameTick tick = this.getCurrentTick().rewind(ticks);
            this.goTo(tick);
        }

        /// <summary>
        /// Gets a list of the queued events
        /// </summary>
        /// <returns>A list of the events in the future event queue</returns>
        public List<GameEvent> getQueuedEvents()
        {
            List<GameEvent> gameEvents = new List<GameEvent>();
            foreach(GameEvent gameEvent in this.futureEventQueue.getQueue()){
                if (gameEvent != null){
                    gameEvents.Add(gameEvent);
                }
            }
            return gameEvents;
        }
    }
}
