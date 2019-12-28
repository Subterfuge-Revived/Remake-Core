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
        // List of currently active subs
        private List<Sub> activeSubs = new List<Sub>();
        // List of outposts
        private List<Outpost> outposts = new List<Outpost>();
        // List of players
        private List<Player> players = new List<Player>();
        
        // current time and start time
        public GameTick currentTick;
        private GameTick startTime;

        /// <summary>
        /// Constructor of a gamestate. Initializes an empty game.
        /// </summary>
        public GameState()
        {
            // Set the start time to the time the game was initialized at and set the current tick
            this.startTime = new GameTick(new DateTime(), 0);
            this.currentTick = this.startTime;

            // Setup some initial settings for the game state.
            Player player1 = new Player(1);
            Player player2 = new Player(2);
            Player player3 = new Player(3);
            Player player4 = new Player(4);
            Player player5 = new Player(5);
            Player player6 = new Player(6);

            this.players.Add(player1);
            this.players.Add(player2);
            this.players.Add(player3);
            this.players.Add(player4);
            this.players.Add(player5);
            this.players.Add(player6);
        }

        /// <summary>
        /// Gets the GameState's current game tick.
        /// </summary>
        /// <returns>The current game tick</returns>
        public GameTick getCurrentTick()
        {
            return this.currentTick;
        }

        /// <summary>
        /// Gets the game's start time
        /// </summary>
        /// <returns>The game's start time</returns>
        public GameTick getStartTick()
        {
            return this.startTime;
        }
        
        /// <summary>
        /// Advances the gamestate by one tick. Does NOT apply any time machine actions.
        /// </summary>
        /// <returns>The next tick</returns>
        public GameTick goToNextTick()
        {
            this.currentTick = this.currentTick.getNextTick();
            return this.currentTick;
        }

        /// <summary>
        /// Returns a list of all active subs in the game
        /// </summary>
        /// <returns>A list of the active subs</returns>
        public List<Sub> getSubList()
        {
            return this.activeSubs;
        }

        /// <summary>
        /// Returns a list of all outposts in the game
        /// </summary>
        /// <returns>A list of all outposts</returns>
        public List<Outpost> getOutposts()
        {
            return this.outposts;
        }

        /// <summary>
        /// Returns a list of all the players in the game
        /// </summary>
        /// <returns>A list of all players</returns>
        public List<Player> getPlayers()
        {
            return this.players;
        }

        /// <summary>
        /// Determines if a given sub exists in the GameState. This method is useful for checking if an event
        /// is holding a reference to an object that has lost a combat and was removed from the game or if the object
        /// has survived and is still in the game.
        /// </summary>
        /// <param name="sub">The sub to check exists</param>
        /// <returns>if the sub exists in the GameState</returns>
        public bool subExists(Sub sub)
        {
            return this.activeSubs.Contains(sub);
        }

        /// <summary>
        /// Launches a sub (or brings a sub back into the game if reversing). Adding the sub to the active sub list
        /// </summary>
        /// <param name="sub">The sub to add to the GameState</param>
        public void addSub(Sub sub)
        {
            this.activeSubs.Add(sub);
        }

        /// <summary>
        /// Revmoes a sub from the list of active subs
        /// </summary>
        /// <param name="sub">The sub to remove from the game</param>
        public void removeSub(Sub sub)
        {
            this.activeSubs.Remove(sub);
        }

        /// <summary>
        /// Determine all subs that are on a path between two outpost desinations
        /// </summary>
        /// <param name="source">The source outpost</param>
        /// <param name="destination">The destination outpost</param>
        /// <returns>A list of all subs sailing between the source and destination</returns>
        public List<Sub> getSubsOnPath(Outpost source, Outpost destination)
        {
            List<Sub> subsOnPath = new List<Sub>();

            // Check if a sub is on that path

            // This logic is not nessicarily true..
            // If a sub has a navigator this logic kind of falls apart.
            foreach (Sub sub in this.activeSubs)
            {

                if (sub.getDestination() == destination || sub.getDestination() == source)
                {
                    if (sub.getSource() == source || sub.getSource() == destination)
                    {
                        // Sub is on the path.
                        subsOnPath.Add(sub);
                    }
                }
            }
            return subsOnPath;
        }

        /// <summary>
        /// Get a list of the specific player's subs
        /// </summary>
        /// <param name="player">The player to get the subs of</param>
        /// <returns>A list of the player's subs</returns>
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

        /// <summary>
        /// Get a list of the specific player's outposts
        /// </summary>
        /// <param name="player">The player to get the outposts of</param>
        /// <returns>A list of the player's controlled outposts</returns>
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

        /// <summary>
        /// Determines if the referenced outpost exists.
        /// </summary>
        /// <param name="outpost">The outpost to check</param>
        /// <returns>If the referenced outpost exists</returns>
        public bool outpostExists(Outpost outpost)
        {
            return this.outposts.Contains(outpost);
        }

        /// <summary>
        /// Determines if the player is in the game
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>If the player exists in the game</returns>
        public bool playerExists(Player player)
        {
            return this.players.Contains(player);
        }
    }
}
