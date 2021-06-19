using System;
using System.Collections.Generic;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.NaturalGameEvents.outpost;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

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
        /// Time machine instance which controls the game state
        /// </summary>
        public TimeMachine TimeMachine;
        
        /// <summary>
        /// The game configuration. Determines things like the map generation config,
        /// if the game is multiplayer, how many players are involved, etc.
        /// </summary>
        public GameConfiguration Configuration { get; } = null;

        public GameMode GameMode { get; set; } = GameMode.MINING;

        /// <summary>
        /// Creates a new game. Does not generate outposts.
        /// Only use this constructor for testing purposes, it sets up a `GameState` and `TimeMachine` instance
        /// without any data.
        /// </summary>
        public Game()
        {
            // Create a generic game configuration with one player.
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            Configuration = new GameConfiguration(players, DateTime.Now, new MapConfiguration(players));
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
            Configuration = gameConfiguration;
            
            // Creates a new game state and makes a time machine to reference the state
            GameState state = new GameState(gameConfiguration);
            TimeMachine = new TimeMachine(state);

            // Creates the map generator with a random seed
            MapGenerator mapGenerator = new MapGenerator(gameConfiguration.MapConfiguration);
            
            // Generate the map
            List<Outpost> generatedOutposts = mapGenerator.GenerateMap();

            // Add the outposts to the map
            state.GetOutposts().AddRange(generatedOutposts);

            // All owned factories should start producing drillers
            foreach (Outpost o in generatedOutposts)
            {
                if (o is Factory && o.GetOwner() != null)
                {
                    Factory f = (Factory)o;
                    TimeMachine.AddEvent(new FactoryProduction(f, f.GetTicksToFirstProduction()));
                }
            }
        }

        public void LoadGameEvents(List<GameEventModel> gameEvents)
        {
            gameEvents
                .ConvertAll<GameEvent>(m => GameEventFactory.parseGameEvent(m))
                .ForEach( parsedEvent => TimeMachine.AddEvent(parsedEvent) );
        }

        /// <summary>
        /// Determines if the game is in a game over state. In Mining, ties are broken by
        /// whoever happens to be first in the list of players.
        /// </summary>
        /// <returns>null if the game is not over, or the Player who has won if it is over.</returns>
        public Player IsGameOver()
        {
            switch (GameMode)
            {
                case GameMode.MINING:
                    foreach (Player p in TimeMachine.GetState().GetPlayers())
                    {
                        if (!p.IsEliminated() && p.GetNeptunium() >= Constants.MINING_NEPTUNIUM_TO_WIN)
                        {
                            return p;
                        }
                    }
                    return null;

				case GameMode.DOMINATION:
                    foreach (Player p in TimeMachine.GetState().GetPlayers())
                    {
                        if (!p.IsEliminated() && TimeMachine.GetState().GetPlayerOutposts(p).Count > TimeMachine.GetState().GetOutposts().Count / 2)
                        {
                            return p;
                        }
                    }
                    return null;

                // Other cases to be implemented

                default:
                    return null;
            }
        }
    }
}
