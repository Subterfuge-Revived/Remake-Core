using System.Collections.Generic;
using System.Linq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;
using Subterfuge.Remake.Core.Generation;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Core
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
        /// The random number generated used for all randomly selected events within the game.
        /// This includes things like outpost generation and specialist pool randomization.
        /// </summary>
        public SeededRandom SeededRandom;

        /// <summary>
        /// Current Game configuration
        /// </summary>
        public GameConfiguration GameConfiguration;

        public static Game FromGameConfiguration(GameConfiguration gameConfiguration)
        {
            return new Game(gameConfiguration);
        }

        public static Game Bare()
        {
            RftVector.Map = new Rft(1000, 1000);
            return new Game();
        }

        private Game()
        {
            GameConfiguration = new GameConfiguration();
            SetupGameFromConfiguration(GameConfiguration, false);
        }

        /// <summary>
        /// Creates a new game using the provided GameConfiguration. Calling this constructor will trigger
        /// map generation and generate the map based on the GameConfiguration that was passed into the game.
        /// </summary>
        /// <param name="gameConfiguration">Settings that determine how the game should be configured during generation.</param>
        private Game(GameConfiguration gameConfiguration)
        {
            SetupGameFromConfiguration(gameConfiguration, true);
        }

        private void SetupGameFromConfiguration(GameConfiguration gameConfiguration, bool shouldGenerateMap = true)
        {
            GameConfiguration = gameConfiguration;
            SeededRandom = new SeededRandom(gameConfiguration.MapConfiguration.Seed);
            
            // Creates a new game state and makes a time machine to reference the state
            GameState state = new GameState(gameConfiguration.PlayersInLobby.Select(it => new Player(it)).ToList());
            TimeMachine = new TimeMachine(state);
            
            // Create a global currency event.
            GlobalCurrencyProductionEvent.SpawnNewCurrencyEvent(TimeMachine);

            GenerateMap(shouldGenerateMap);
            
            // Create a global currency event.
            GlobalCurrencyProductionEvent.SpawnNewCurrencyEvent(TimeMachine); 
        }

        private void GenerateMap(bool shouldGenerate)
        {
            if (shouldGenerate)
            {
                // Creates the map generator with a random seed
                MapGenerator mapGenerator = new MapGenerator(
                    GameConfiguration.MapConfiguration,
                    TimeMachine.GetState().GetPlayers(),
                    TimeMachine,
                    SeededRandom
                );
            
                // Generate the map
                List<Outpost> generatedOutposts = mapGenerator.GenerateMap();

                // Add the outposts to the map
                TimeMachine.GetState().GetOutposts().AddRange(generatedOutposts);
            }
        }

        public void LoadGameEvents(List<GameRoomEvent> gameEvents)
        {
            gameEvents
                .ConvertAll<GameEvent>(m => GameEventFactory.ParseGameEvent(m))
                .ForEach(parsedEvent =>
                {
                    if (parsedEvent != null)
                    {
                        TimeMachine.AddEvent(parsedEvent);
                    }
                });
        }

        /// <summary>
        /// Determines if the game is in a game over state. In Mining, ties are broken by
        /// whoever happens to be first in the list of players.
        /// </summary>
        /// <returns>null if the game is not over, or the Player who has won if it is over.</returns>
        public Player IsGameOver()
        {
            GameEndEvent? endEvent = (GameEndEvent)this.TimeMachine.GetPastEvents().FirstOrDefault(it => it is GameEndEvent);
            if (endEvent != null)
            {
                return this.TimeMachine.GetState()
                    .GetPlayers()
                    .FirstOrDefault(player => player.GetId() == endEvent.GetEventData().WinningPlayer.Id);
            }
            switch (GameConfiguration.GameSettings.Goal)
            {
                case Goal.Mining:
                    foreach (Player p in TimeMachine.GetState().GetPlayers())
                    {
                        if (!p.IsEliminated() && p.GetNeptunium() >= Constants.MiningNeptuniumToWin)
                        {
                            return p;
                        }
                    }

                    return null;

                case Goal.Domination:
                    foreach (Player p in TimeMachine.GetState().GetPlayers())
                    {
                        if (!p.IsEliminated() && TimeMachine.GetState().GetPlayerOutposts(p).Count >
                            TimeMachine.GetState().GetOutposts().Count / 2)
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
