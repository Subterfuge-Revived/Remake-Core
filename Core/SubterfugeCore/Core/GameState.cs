using System;
using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Interfaces;
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
        /// <summary>
        /// A list of the currently existing subs
        /// </summary>
        private List<Sub> _activeSubs = new List<Sub>();
        
        /// <summary>
        /// A list of the current oupost states
        /// </summary>
        private List<Outpost> _outposts = new List<Outpost>();
        
        /// <summary>
        /// A list of the players in the game
        /// </summary>
        private List<Player> _players = new List<Player>();
        
        /// <summary>
        /// The current game tick
        /// </summary>
        public GameTick CurrentTick;
        
        /// <summary>
        /// The time the game started.
        /// </summary>
        private GameTick _startTime;
        
        /// <summary>
        /// Constructs a new instance of a GameState to represent the game.
        /// Generally, the `Game` and `TimeMachine` objects will handle the creation and management of the GameState
        /// you will likely never need to use this constructor.
        /// </summary>
        /// <param name="configuration">The Game configuration used to create the game. Required to setup the player list.</param>
        public GameState(GameConfiguration configuration)
        {
            // Set the start time to the time the game was initialized at and set the current tick
            this._startTime = new GameTick();
            this.CurrentTick = this._startTime;
            
            // Set the players.
            this._players = configuration.Players;
        }

        /// <summary>
        /// Gets the GameTick that this GameState object is representing.
        /// </summary>
        /// <returns>The GameTick that the GameState represents.</returns>
        public GameTick GetCurrentTick()
        {
            return this.CurrentTick;
        }

        /// <summary>
        /// Gets the game's start time
        /// </summary>
        /// <returns>The game's start time</returns>
        public GameTick GetStartTick()
        {
            return this._startTime;
        }
        
        /// <summary>
        /// Advances the GameState by one tick. IMPORTANT: This does NOT apply any time machine actions.
        /// </summary>
        /// <returns>The next GameTick</returns>
        public GameTick GoToNextTick()
        {
            this.CurrentTick = this.CurrentTick.GetNextTick();
            return this.CurrentTick;
        }

        /// <summary>
        /// Returns a list of all active subs in the game
        /// </summary>
        /// <returns>A list of the active subs</returns>
        public List<Sub> GetSubList()
        {
            return this._activeSubs;
        }

        /// <summary>
        /// Returns a list of all outposts in the game
        /// </summary>
        /// <returns>A list of all outposts</returns>
        public List<Outpost> GetOutposts()
        {
            return this._outposts;
        }

        /// <summary>
        /// Returns a list of all the players in the game
        /// </summary>
        /// <returns>A list of all players</returns>
        public List<Player> GetPlayers()
        {
            return this._players;
        }

        /// <summary>
        /// Determines if a given sub exists in the GameState. This method is useful for checking if an event
        /// is holding a reference to an object that has lost a combat and was removed from the game or if the object
        /// has survived and is still in the game.
        /// </summary>
        /// <param name="sub">The sub to check exists</param>
        /// <returns>if the sub exists in the GameState</returns>
        public bool SubExists(Sub sub)
        {
            return this._activeSubs.Contains(sub);
        }

        /// <summary>
        /// Launches a sub (or brings a sub back into the game if reversing). Adding the sub to the active sub list
        /// </summary>
        /// <param name="sub">The sub to add to the GameState</param>
        public void AddSub(Sub sub)
        {
            this._activeSubs.Add(sub);
        }

        /// <summary>
        /// Revmoes a sub from the list of active subs
        /// </summary>
        /// <param name="sub">The sub to remove from the game</param>
        public void RemoveSub(Sub sub)
        {
            this._activeSubs.Remove(sub);
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
            foreach (Sub sub in this._activeSubs)
            {

                if (sub.GetDestination() == destination || sub.GetDestination() == source)
                {
                    if (sub.GetSource() == source || sub.GetSource() == destination)
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
        public List<Sub> GetPlayerSubs(Player player)
        {
            List<Sub> playerSubs = new List<Sub>();
            foreach(Sub sub in this._activeSubs)
            {
                if(sub.GetOwner() == player)
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
        public List<Outpost> GetPlayerOutposts(Player player)
        {
            List<Outpost> playerOutposts = new List<Outpost>();
            foreach (Outpost outpost in this._outposts)
            {
                if (outpost.GetOwner() == player)
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
        public ICombatable GetCombatableById(string id)
        {
            foreach (Outpost outpost in this._outposts)
            {
                if (outpost.GetId().Equals(id))
                {
                    return outpost;
                }
            }
            
            foreach (Sub sub in this._activeSubs)
            {
                if (sub.GetId().Equals(id))
                {
                    return sub;
                }
            }

            return null;
        }



        /// <summary>
        /// Gets a sub by its Guid
        /// </summary>
        /// <param name="guid">The guid of a sub to find.</param>
        /// <returns>The sub with the specified guid. Null if no sub exists with the specified Guid.</returns>
        public Sub GetSubById(string id)
        {
            foreach (Sub sub in this._activeSubs)
            {
                if (sub.GetId() == id)
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
        public bool OutpostExists(Outpost outpost)
        {
            return this._outposts.Contains(outpost);
        }

        /// <summary>
        /// Determines if the player is in the game
        /// </summary>
        /// <param name="player">The player to check</param>
        /// <returns>If the player exists in the game</returns>
        public bool PlayerExists(Player player)
        {
            return this._players.Contains(player);
        }

        /// <summary>
        /// Replaces an outpost in the game state with another outpost. The two outposts should have the same Outpost-level fields, only differing in subclass.
        /// </summary>
        /// <param name="remove">An outpost in the game state to remove.</param>
        /// <param name="add">An outpost not in the game state to add.</param>
        /// <returns>True if the replacement was successful, and false otherwise.</returns>
        public bool ReplaceOutpost(Outpost remove, Outpost add)
        {
            if (_outposts.Contains(remove) && !_outposts.Contains(add))
            {
                _outposts.Remove(remove);
                _outposts.Add(add);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a list of all specilists in the game
        /// </summary>
        /// <returns>A list of all specliasts in the game</returns>
        public List<Specialist> GetSpecialists()
        {
            List<Specialist> specialists = new List<Specialist>();
            foreach(Outpost o in this._outposts)
            {
                specialists.AddRange(o.GetSpecialistManager().GetSpecialists());
            }

            foreach (Sub s in _activeSubs)
            {
                specialists.AddRange(s.GetSpecialistManager().GetSpecialists());
            }

            return specialists;
        }

        /// <summary>
        /// Gets a list of a player's speciliasts
        /// </summary>
        /// <param name="player">The player to get specialists for</param>
        /// <returns>A list of the player's specialists</returns>
        public List<Specialist> GetPlayerSpecialists(Player player)
        {
            List<Specialist> specialists = new List<Specialist>();
            foreach(Outpost o in this._outposts)
            {
                if (o.GetOwner() == player)
                {
                    specialists.AddRange(o.GetSpecialistManager().GetSpecialists());
                }
            }

            foreach (Sub s in _activeSubs)
            {
                if (s.GetOwner() == player)
                {
                    specialists.AddRange(s.GetSpecialistManager().GetSpecialists());
                }
            }

            return specialists;
        }

        /// <summary>
        /// Determine if the player is alive
        /// </summary>
        /// <param name="player">The player</param>
        /// <returns>If the player is alive</returns>
        public bool isPlayerAlive(Player player)
        {
            List<Specialist> specs = GetPlayerSpecialists(player);
            return specs.Find(spec => spec is Queen).IsCaptured;
        }

        /// <summary>
        /// Gets the player's driller count
        /// </summary>
        /// <param name="player">The player's driller count</param>
        /// <returns></returns>
        public int getPlayerDrillerCount(Player player)
        {
            return GetPlayerOutposts(player).Sum( it => it.GetDrillerCount()) + GetPlayerSubs(player).Sum(it => it.GetDrillerCount());
        }

        /// <summary>
        /// Determines the player's driller capacity
        /// </summary>
        /// <param name="player">player</param>
        /// <returns>The player's driller capacity</returns>
        public int getPlayerDrillerCapacity(Player player)
        {
            return (GetPlayerOutposts(player)
                       .FindAll(outpost => outpost.GetOutpostType().Equals(OutpostType.Generator)).Count *
                   Constants.BASE_GENERATOR_CAPACITY) + Constants.BASE_DRILLER_CAPACITY;
        }

        /// <summary>
        /// Determines how many extra drillers the specified player's electrical
        /// can support.
        /// </summary>
        /// <param name="player">player</param>
        /// <returns>The amount of extra drillers the player can have.
        /// If it returns 0 or less, the player is out of electricity. </returns>
        public int GetExtraDrillerCapcity(Player player)
        {
            return getPlayerDrillerCapacity(player) - getPlayerDrillerCount(player);
        }

        /// <summary>
        /// Determines whether a position on the map is visible to the
        /// specified player.
        /// </summary>
        /// <param name="position">position</param>
        /// <param name="player">player</param>
        /// <returns>True if the position is visible and false otherwise.</returns>
        public bool isInVisionRange(IPosition position, Player player)
        {
            foreach(Outpost o in GetPlayerOutposts(player))
            {
                if (o.isInVisionRange(CurrentTick, position))
                {
                    return true;
                }
            }

            foreach (Sub s in GetPlayerSubs(player))
            {
                if (s.isInVisionRange(CurrentTick, position))
                {
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Gets a list of all ICombatable instances of a particular player.
        /// </summary>
        /// <param name="player">The player to get combatables for</param>
        /// <returns>The list of the player's property</returns>
        public List<ICombatable> getPlayerTargetables(Player player)
        {
            List<ICombatable> targetables = new List<ICombatable>();
            targetables.AddRange(this.GetPlayerOutposts(player));
            targetables.AddRange(this.GetPlayerSubs(player));
            return targetables;
        }

        /// <summary>
        /// Gets a list of all game objects
        /// </summary>
        /// <returns>A list of all game objects.</returns>
        public List<ICombatable> GetAllGameObjects()
        {
            List<ICombatable> gameObjects = new List<ICombatable>();
            gameObjects.AddRange(_activeSubs);
            gameObjects.AddRange(_outposts);
            return gameObjects;
        }
    }
}
