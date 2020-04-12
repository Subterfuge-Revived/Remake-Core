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
        // Globally accessible time machine reference
        public static TimeMachine TimeMachine;

        // temporary control for time machine.
        private bool _forward = true;
        private bool _addedEvents = false;
        public GameConfiguration Configuration { get; } = null;

        /// <summary>
        /// Creates a new game. Does not generate outposts.
        /// Only use this constructor for testing purposes, it sets up a `GameState` and `TimeMachine` instance
        /// without any data.
        /// </summary>
        public Game()
        {
            // Create a generic game configuration
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            players.Add(new Player(2));
            players.Add(new Player(3));
            players.Add(new Player(4));
            players.Add(new Player(5));
            players.Add(new Player(6));
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

    }
}
