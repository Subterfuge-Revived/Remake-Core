using SubterfugeCore.Entities;
using SubterfugeCore.Entities.Base;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Timing;
using System.Collections.Generic;
using SubterfugeCore.Players;
using SubterfugeCore.Core.Timing;
using Microsoft.Xna.Framework;
using System;

namespace SubterfugeCore
{
    /// <summary>
    /// This class holds only the nessecary information that is needed at a single point in time.
    /// This class has nothing to do with time.
    /// </summary>
    public class GameState
    {
        private List<Sub> activeSubs = new List<Sub>();
        private List<Outpost> outposts = new List<Outpost>();
        private List<Player> players = new List<Player>();
        public GameTick currentTick;
        private GameTick startTime;

        public GameState()
        {
            this.startTime = new GameTick(new DateTime(), 0);
            this.currentTick = this.startTime;

            // Setup some initial settings for the game state.
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
        }

        // Copy Constructor
        public GameState(GameState state)
        {
            this.activeSubs = new List<Sub>(state.activeSubs);
            this.outposts = new List<Outpost>(state.outposts);
            this.players = new List<Player>(state.players);
            this.currentTick = new GameTick(state.currentTick);
            this.startTime = new GameTick(state.startTime);
        }

        public GameTick getCurrentTick()
        {
            return this.currentTick;
        }

        public GameTick getStartTick()
        {
            return this.startTime;
        }

        public GameTick goToNextTick()
        {
            this.currentTick = this.currentTick.getNextTick();
            return this.currentTick;
        }

        public List<Sub> getSubList()
        {
            return this.activeSubs;
        }

        public List<Outpost> getOutposts()
        {
            return this.outposts;
        }

        public List<Player> getPlayers()
        {
            return this.players;
        }

        // Determine if the sub exists in the game
        public bool subExists(Sub sub)
        {
            return this.activeSubs.Contains(sub);
        }

        // Launch a sub
        public bool addSub(Sub sub)
        {
            if (sub.getSourceOutpost().hasDrillers(sub.getDrillerCount()))
            {
                this.activeSubs.Add(sub);
                return true;
            }
            return false;
        }

        // Remove a sub from the game.
        public void removeSub(Sub sub)
        {
            this.activeSubs.Remove(sub);
        }

        // Determine all of the subs that are on a path between the source and destination outpost.
        public List<Sub> getSubsOnPath(Outpost source, Outpost destination)
        {
            List<Sub> subsOnPath = new List<Sub>();

            // Check if a sub is on that path
            foreach (Sub sub in this.activeSubs)
            {

                if (sub.getDestination() == destination || sub.getDestination() == source)
                {
                    if (sub.getSourceOutpost() == source || sub.getSourceOutpost() == destination)
                    {
                        // Sub is on the path.
                        subsOnPath.Add(sub);
                    }
                }
            }
            return subsOnPath;
        }

        // Get a list of a player's subs
        public List<Sub> getPlayerSubs(Player player)
        {
            List<Sub> playerSubs = new List<Sub>();
            foreach(Sub sub in this.activeSubs)
            {
                if(sub.getOwner() == player)
                {
                    playerSubs.Add(sub);
                }
            }
            return playerSubs;
        }

        // Get player outposts
        public List<Outpost> getPlayerOutposts(Player player)
        {
            List<Outpost> playerOutposts = new List<Outpost>();
            foreach (Outpost outpost in this.outposts)
            {
                if (outpost.getOwner() == player)
                {
                    playerOutposts.Add(outpost);
                }
            }
            return playerOutposts;
        }
    }
}
