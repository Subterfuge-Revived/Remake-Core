using System;
using System.Collections.Generic;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core
{
    /// <summary>
    /// This class holds only the nessecary information that is needed to render the map at any given point in time.
    /// This class contains a list of all subs, outposts, and players in the game. Get an instance of this class by
    /// using `Game.timeMachine.getState()` to get the current state of the game.
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
        /// Constructs a new instance of a GameState to represent the game.
        /// Generally, the `Game` and `TimeMachine` objects will handle the creation and management of the GameState
        /// you will likely never need to use this constructor.
        /// </summary>
        /// <param name="configuration">The Game configuration used to create the game. Required to setup the player list.</param>
        public GameState(GameConfiguration configuration)
        {
            // Set the start time to the time the game was initialized at and set the current tick
            this.startTime = new GameTick(new DateTime(), 0);
            this.currentTick = this.startTime;
            
            // Set the players.
            this.players = configuration.players;
        }

        /// <summary>
        /// Gets the GameTick that this GameState object is representing.
        /// </summary>
        /// <returns>The GameTick that the GameState represents.</returns>
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
        /// Advances the GameState by one tick. IMPORTANT: This does NOT apply any time machine actions.
        /// </summary>
        /// <returns>The next GameTick</returns>
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
        /// Gets an outpost by its GUID
        /// </summary>
        /// <param name="guid">The Guid of the outpost you want to obtain.</param>
        /// <returns>The outpost matching the input Guid. Null if no results.</returns>
        public Outpost getOutpostByGuid(Guid guid)
        {
            foreach (Outpost outpost in this.outposts)
            {
                if (outpost.getGuid() == guid)
                {
                    return outpost;
                }
            }

            return null;
        }



        /// <summary>
        /// Gets a sub by its Guid
        /// </summary>
        /// <param name="guid">The guid of a sub to find.</param>
        /// <returns>The sub with the specified guid. Null if no sub exists with the specified Guid.</returns>
        public Sub getSubByGuid(Guid guid)
        {
            foreach (Sub sub in this.activeSubs)
            {
                if (sub.getGuid() == guid)
                {
                    return sub;
                }
            }
            return null;
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
