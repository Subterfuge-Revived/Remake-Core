using System.Collections.Generic;
using System.Linq;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents;
using Subterfuge.Remake.Core.Generation;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

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
        /// The specialist pool for the game. Provides a list of the possible specialists that can be obtained
        /// as well as their configuration.
        /// </summary>
        public static GlobalSpecialistPool SpecialistConfigurationPool = new GlobalSpecialistPool();

        /// <summary>
        /// The random number generated used for all randomly selected events within the game.
        /// This includes things like outpost generation and specialist pool randomization.
        /// </summary>
        public static SeededRandom SeededRandom;

        /// <summary>
        /// Current Game configuration
        /// </summary>
        public static GameConfiguration GameConfiguration;

        private CurrencyProducer CurrencyProducer;

        /// <summary>
        /// Creates a new game using the provided GameConfiguration. Calling this constructor will trigger
        /// map generation and generate the map based on the GameConfiguration that was passed into the game.
        /// </summary>
        /// <param name="gameConfiguration">Settings that determine how the game should be configured during generation.</param>
        public Game(GameConfiguration gameConfiguration)
        {
            GameConfiguration = gameConfiguration;
            SeededRandom = new SeededRandom();
            
            // Creates a new game state and makes a time machine to reference the state
            GameState.GameState state = new GameState.GameState(gameConfiguration.PlayersInLobby.Select(it => new Player(it)).ToList());
            TimeMachine = new TimeMachine(state);

            // Creates the map generator with a random seed
            MapGenerator mapGenerator = new MapGenerator(gameConfiguration.MapConfiguration, state.GetPlayers(), TimeMachine);
            
            // Generate the map
            List<Outpost> generatedOutposts = mapGenerator.GenerateMap();

            // Add the outposts to the map
            state.GetOutposts().AddRange(generatedOutposts);
            
            // Populate the specialist pool
            CurrencyProducer = new CurrencyProducer(TimeMachine);
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
