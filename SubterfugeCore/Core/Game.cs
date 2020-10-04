using System.Collections.Generic;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core
{
    /**
     * This game server class will hold all of the game logic.
     * This includes holding the game state, as well as being able to interpolate the locations of all a player's outposts, 
     * subs, etc.
     * 
     * No graphics will be used within this project. The graphics engine will need to reference the objects within this class to
     * determine how to draw.
     */
    public class Game
    {
        /// <summary>
        /// Globally accessible variable. Allows accessing the time machine.
        /// Try not to access this in a static manner to perform updates. Only use this static reference
        /// from unity.
        /// </summary>
        public static TimeMachine TimeMachine;
        
        /// <summary>
        /// The game configuration for map generation.
        /// </summary>
        public GameConfiguration Configuration { get; } = null;

        /// <summary>
        /// The game mode being played.
        /// </summary>
        public static GameMode GameMode { get; set; } = GameMode.MINING;

        /// <summary>
        /// Creates a new game. Does not generate outposts.
        /// Only use this constructor for testing purposes, it sets up a `GameState` and `TimeMachine` instance
        /// without any data.
        /// </summary>
        public Game()
        {
            // Create a generic game configuration with one player.
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            Configuration = new GameConfiguration(players);
            // Creates a new game state and makes a time machine to reference the state
            GameState state = new GameState(Configuration);
            TimeMachine = new TimeMachine(state);
        }

        /// <summary>
        /// Creates a new game using the provided GameConfiguration. Calling this constructor will trigger
        /// map generation and generate the map based on the GameConfiguration that was passed into the game.
        /// </summary>
        /// <param name="gameConfiguration">Settings that determine how the game should be configured during generation.</param>
        public Game(GameConfiguration gameConfiguration)
        {
            // Creates a new game state and makes a time machine to reference the state
            GameState state = new GameState(gameConfiguration);
            TimeMachine = new TimeMachine(state);

            // Creates the map generator with a random seed
            MapGenerator mapGenerator = new MapGenerator(gameConfiguration);
            
            // Generate the map
            List<Outpost> outpostsToGenerate = mapGenerator.GenerateMap();
            
            // Add factory driller production events to the time machine.
            // TODO: Make this better.
            foreach(Outpost o in outpostsToGenerate)
            {
                if (o.GetOutpostType() == OutpostType.Factory) {
                    TimeMachine.AddEvent(new FactoryProduceDrillersEvent(o, state.GetCurrentTick().Advance(36)));
                }
            }
            
            // Add the outposts to the map
            state.GetOutposts().AddRange(outpostsToGenerate);
        }

        public static bool IsGameOver()
        {
            switch (GameMode)
            {
                case GameMode.MINING:
                    foreach(Player p in TimeMachine.GetState().GetPlayers())
                    {
                        if (p.getNeptunium() > 200)
                        {
                            return true;
                        }                        
                    }

                    return false;
                case GameMode.DOMINATION:
                    
                    foreach(Player p in TimeMachine.GetState().GetPlayers())
                    {
                        if (TimeMachine.GetState().GetPlayerOutposts(p).Count > 40)
                        {
                            return true;
                        }                        
                    }
                    return false;
                case GameMode.PUZZLE:
                    // TODO
                    return false;
                case GameMode.SANDBOX:
                    return false;
                default:
                    return false;
            }
        }

    }
}
