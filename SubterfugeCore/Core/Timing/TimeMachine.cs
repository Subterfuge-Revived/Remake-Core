using SubterfugeCore.Entities.Base;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Timing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Timing
{
    public class TimeMachine
    {
        // List of known evnets
        private ReversePriorityQueue<GameEvent> pastEventQueue = new ReversePriorityQueue<GameEvent>();
        private PriorityQueue<GameEvent> futureEventQueue = new PriorityQueue<GameEvent>();

        // Current representation of the game state
        private GameState gameState;

        // To determine the current position of the time machine.
        public GameTick currentTick;
        private GameTick startTime;

        // Constructor for the time machine.
        public TimeMachine(GameState state)
        {
            startTime = state.getStartTick();
            currentTick = state.getCurrentTick();
            gameState = state;
        }

        public GameState getState()
        {
            return this.gameState;
        }

        public void addEvent(GameEvent gameEvent)
        {
            this.futureEventQueue.Enqueue(gameEvent);
        }

        public void removeEvent(GameEvent gameEvent)
        {
            this.futureEventQueue.remove(gameEvent);
        }

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

        public GameTick getCurrentTick()
        {
            return this.currentTick;
        }

        public GameTick getStartTick()
        {
            return this.startTime;
        }

        public void goTo(GameEvent eventOfInterest)
        {
            this.goTo(eventOfInterest.getTick());
        }

    }
}
