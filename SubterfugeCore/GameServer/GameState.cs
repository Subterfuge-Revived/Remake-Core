using SubterfugeCore.Entities;
using SubterfugeCore.Entities.Base;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Timing;
using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework;
using SubterfugeCore.Timing.PriorityQueue;

namespace SubterfugeCore
{
    /// <summary>
    /// This class holds information about a game's current state.
    /// </summary>
    public class GameState
    {
        private PriorityQueue<GameEvent> pastEventQueue = new PriorityQueue<GameEvent>();
        private PriorityQueue<GameEvent> futureEventQueue = new PriorityQueue<GameEvent>();
        private List<GameObject> activeSubs = new List<GameObject>();
        private List<GameObject> outposts = new List<GameObject>();
        public static GameTick currentTick;
        private GameTick startTime;

        public GameState()
        {
            currentTick = new GameTick(new DateTime(), 0);

            Outpost outpost1 = new Outpost(new Vector2(100, 100));
            Outpost outpost2 = new Outpost(new Vector2(800, 800));
            Outpost outpost3 = new Outpost(new Vector2(600, 1200));
            Outpost outpost4 = new Outpost(new Vector2(400, 1000));

            SubLaunchEvent launchEvent1 = new SubLaunchEvent(currentTick.advance(100), outpost1, 1, outpost2);
            SubLaunchEvent launchEvent2 = new SubLaunchEvent(currentTick.advance(200), outpost1, 2, outpost3);
            SubLaunchEvent launchEvent3 = new SubLaunchEvent(currentTick.advance(300), outpost1, 3, outpost4);

            SubLaunchEvent launchEvent4 = new SubLaunchEvent(currentTick.advance(400), outpost2, 4, outpost1);
            SubLaunchEvent launchEvent5 = new SubLaunchEvent(currentTick.advance(500), outpost2, 5, outpost3);
            SubLaunchEvent launchEvent6 = new SubLaunchEvent(currentTick.advance(600), outpost2, 6, outpost4);

            SubLaunchEvent launchEvent7 = new SubLaunchEvent(currentTick.advance(700), outpost3, 7, outpost1);
            SubLaunchEvent launchEvent8 = new SubLaunchEvent(currentTick.advance(800), outpost3, 8, outpost2);
            SubLaunchEvent launchEvent9 = new SubLaunchEvent(currentTick.advance(900), outpost3, 9, outpost4);

            this.addEvent(launchEvent1);
            this.addEvent(launchEvent2);
            this.addEvent(launchEvent3);

            this.addEvent(launchEvent4);
            this.addEvent(launchEvent5);
            this.addEvent(launchEvent6);
            this.addEvent(launchEvent7);
            this.addEvent(launchEvent8);
            this.addEvent(launchEvent9);

            //activeSubs.Add(new Sub(new Vector2(0, 0), new Vector2(1, 1), new GameTick(new DateTime(), 0)));
            outposts.Add(outpost1);
            outposts.Add(outpost2);
            outposts.Add(outpost3);
            outposts.Add(outpost4);
        }

        public void interpolateTick(GameTick tick)
        {
            if(tick > currentTick)
            {
                bool evaluating = true;
                while (evaluating)
                {

                    if (futureEventQueue.Count() > 0)
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
                    if (pastEventQueue.Count() > 0)
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
            currentTick = tick;
        }

        public List<GameObject> getSubList()
        {
            return this.activeSubs;
        }

        public List<GameObject> getOutposts()
        {
            return this.outposts;
        }

        public void update()
        {
            this.interpolateTick(currentTick.getNextTick());
        }

        public static GameTick getCurrentTick()
        {
            return currentTick;
        }

        public GameTick getStartTick()
        {
            return this.startTime;
        }

        public void addEvent(GameEvent gameEvent)
        {
            this.futureEventQueue.Enqueue(gameEvent);
            gameEvent.getResultingEvents().ForEach(resultEvent => { this.futureEventQueue.Enqueue(resultEvent); });
        }
    }
}
