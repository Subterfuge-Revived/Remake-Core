using SubterfugeCore.Entities;
using SubterfugeCore.Entities.Base;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Timing;
using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.Xna.Framework;
using SubterfugeCore.Players;
using SubterfugeCore.Components.Outpost;

namespace SubterfugeCore
{
    /// <summary>
    /// This class holds information about a game's current state.
    /// </summary>
    public class GameState
    {
        private ReversePriorityQueue<GameEvent> pastEventQueue = new ReversePriorityQueue<GameEvent>();
        private PriorityQueue<GameEvent> futureEventQueue = new PriorityQueue<GameEvent>();
        private List<GameObject> activeSubs = new List<GameObject>();
        private List<GameObject> outposts = new List<GameObject>();
        private List<Player> players = new List<Player>();
        public GameTick currentTick;
        private GameTick startTime;

        // Temp
        private bool forward = true;
        private bool setup = false;
        private bool addedEvent = false;

        public GameState()
        {
            startTime = new GameTick(new DateTime(), 0);
            currentTick = startTime;
        }

        public void interpolateTick(GameTick tick)
        {
            if(tick > currentTick)
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
                        if (pastEventQueue.Peek().getTick() >= tick)
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

        public List<Player> getPlayers()
        {
            return this.players;
        }

        public void update()
        {
            if (!setup)
            {
                Player player1 = new Player(1);
                Player player2 = new Player(2);

                this.players.Add(player1);
                this.players.Add(player2);

                Outpost outpost1 = new Outpost(new Vector2(100, 100), player1);
                Outpost outpost2 = new Outpost(new Vector2(300, 100), player2);
                Outpost outpost3 = new Outpost(new Vector2(100, 300), player1);

                Outpost outpost4 = new Outpost(new Vector2(300, 300), player2);

                outposts.Add(outpost1);
                outposts.Add(outpost2);
                outposts.Add(outpost3);
                outposts.Add(outpost4);

                SubLaunchEvent launchEvent1 = new SubLaunchEvent(currentTick.advance(100), outpost1, 30, outpost2);
                this.addEvent(launchEvent1);

                SubLaunchEvent launchEvent2 = new SubLaunchEvent(currentTick.advance(101), outpost2, 1, outpost1);
                this.addEvent(launchEvent2);

                SubLaunchEvent launchEvent3 = new SubLaunchEvent(currentTick.advance(101), outpost3, 30, outpost2);
                this.addEvent(launchEvent3);

                // this.addEvent(launchEvent3);
                // this.addEvent(launchEvent4);
                // this.addEvent(launchEvent5);
                // this.addEvent(launchEvent6);
                // this.addEvent(launchEvent7);
                // this.addEvent(launchEvent8);
                // this.addEvent(launchEvent9);

                setup = true;
            }
            if(currentTick.getTick() > 2000)
            {
                forward = false;
            }
            if(currentTick.getTick() == 108 && !addedEvent)
            {
                SubLaunchEvent launchEvent4 = new SubLaunchEvent(currentTick.advance(2), (Outpost)this.getOutposts()[this.getOutposts().Count - 1], 5, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 1]);
                this.addEvent(launchEvent4);
                addedEvent = true;
                // Testing pirates
                /*
                SubLaunchEvent launchEvent10 = new SubLaunchEvent(currentTick.a
                dvance(1), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 1], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                SubLaunchEvent launchEvent12 = new SubLaunchEvent(currentTick.advance(100), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 1], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                SubLaunchEvent launchEvent11 = new SubLaunchEvent(currentTick.advance(600), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 1], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                SubLaunchEvent launchEvent13 = new SubLaunchEvent(currentTick.advance(1200), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 1], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                SubLaunchEvent launchEvent14 = new SubLaunchEvent(currentTick.advance(1800), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 1], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                SubLaunchEvent launchEvent22 = new SubLaunchEvent(currentTick.advance(2400), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 1], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                SubLaunchEvent launchEvent23 = new SubLaunchEvent(currentTick.advance(3000), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 1], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                SubLaunchEvent launchEvent24 = new SubLaunchEvent(currentTick.advance(3600), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 1], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);

                this.addEvent(launchEvent10);
                this.addEvent(launchEvent11);
                this.addEvent(launchEvent12);
                this.addEvent(launchEvent13);
                this.addEvent(launchEvent14);
                this.addEvent(launchEvent22);
                this.addEvent(launchEvent23);
                this.addEvent(launchEvent24);

                SubLaunchEvent launchEvent15 = new SubLaunchEvent(currentTick.advance(300), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 3], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                SubLaunchEvent launchEvent16 = new SubLaunchEvent(currentTick.advance(900), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 3], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                SubLaunchEvent launchEvent17 = new SubLaunchEvent(currentTick.advance(1500), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 3], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                SubLaunchEvent launchEvent18 = new SubLaunchEvent(currentTick.advance(2100), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 3], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                SubLaunchEvent launchEvent19 = new SubLaunchEvent(currentTick.advance(2700), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 3], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                SubLaunchEvent launchEvent20 = new SubLaunchEvent(currentTick.advance(3300), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 3], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]); 
                SubLaunchEvent launchEvent21 = new SubLaunchEvent(currentTick.advance(3900), (Outpost)GameServer.state.getOutposts()[GameServer.state.getOutposts().Count - 3], 1, (Sub)GameServer.state.getSubList()[GameServer.state.getSubList().Count - 3]);
                this.addEvent(launchEvent15);
                this.addEvent(launchEvent16);
                this.addEvent(launchEvent17);
                this.addEvent(launchEvent18);
                this.addEvent(launchEvent19);
                this.addEvent(launchEvent20);
                this.addEvent(launchEvent21);
                */
            }
            if (currentTick.getTick() == 0)
            {
                forward = true;
            }
            if (forward)
            {
                this.interpolateTick(currentTick.advance(2));
            } else
            {
                this.interpolateTick(currentTick.rewind(2));
            }
        }

        public GameTick getCurrentTick()
        {
            return this.currentTick;
        }

        public GameTick getStartTick()
        {
            return this.startTime;
        }

        public void addEvent(GameEvent gameEvent)
        {
            this.futureEventQueue.Enqueue(gameEvent);
        }

        public void goTo(GameEvent eventOfInterest)
        {
            this.interpolateTick(eventOfInterest.getTick());
        }

        public List<Sub> getSubsOnPath(Outpost source, Outpost destination)
        {
            List<Sub> subsOnPath = new List<Sub>();

            // Check if a sub is on that path
            foreach(Sub sub in this.activeSubs){
           

                if (sub.getDestination() == destination || sub.getDestination() == source)
                {
                    if(sub.getSourceOutpost() == source || sub.getSourceOutpost() == destination)
                    {
                        // Sub is on the path.
                        subsOnPath.Add(sub);
                    }
                }
            }
            return subsOnPath;
        }

        public bool subExists(Sub sub)
        {
            return this.activeSubs.Contains(sub);
        }
    }
}
