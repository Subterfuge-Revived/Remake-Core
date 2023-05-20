using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Generation;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Test
{
    [TestClass]
    public class MapGeneratorTest
    {
        private readonly TestUtils _testUtils = new TestUtils();
        private TimeMachine _timeMachine;
        
        List<Player> players = new List<Player>
        {
            new Player(new SimpleUser() { Id = "1" }),
            new Player(new SimpleUser() { Id = "2" }),
            new Player(new SimpleUser() { Id = "3" }),
            new Player(new SimpleUser() { Id = "4" })
        };

        [TestInitialize]
        public void Setup()
        {
            _timeMachine = new TimeMachine(new GameState(players));
            GameConfiguration config = new TestUtils().GetDefaultGameConfiguration(players);
            new Game(config);
        }

        [TestMethod]
        public void Constructor()
        {

            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaximumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;

            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            Assert.IsNotNull(generator);
        }

        [TestMethod]
        public void GeneratesTheRightNumberOfOutposts()
        {
            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaximumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            Assert.AreEqual(config.PlayersInLobby.Count * (config.MapConfiguration.OutpostsPerPlayer + config.MapConfiguration.DormantsPerPlayer), generatedOutposts.Count);
        }
        
        [TestMethod]
        public void GeneratesOutpostsForAllPlayers()
        {
            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 0;
            config.MapConfiguration.MaximumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            List<Outpost> generatedOutposts = generator.GenerateMap();

            List<Player> playersGenerated = new List<Player>();
            foreach (Outpost outpost in generatedOutposts)
            {
                if(!playersGenerated.Contains(outpost.GetComponent<DrillerCarrier>().GetOwner()))
                    playersGenerated.Add(outpost.GetComponent<DrillerCarrier>().GetOwner());
            }
            
            Assert.AreEqual(config.PlayersInLobby.Count, playersGenerated.Count);
            Assert.AreEqual(config.PlayersInLobby.Count * config.MapConfiguration.OutpostsPerPlayer, generatedOutposts.Count);
        }
        
        [TestMethod]
        public void GeneratesEqualOutpostsForAllPlayers()
        {
            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaximumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            List<Outpost> generatedOutposts = generator.GenerateMap();

            Dictionary<Player, int> outpostCounts = new Dictionary<Player, int>();
            foreach (Outpost outpost in generatedOutposts)
            {
                // Ignore dormant outposts
                if (outpost.GetComponent<DrillerCarrier>().GetOwner() != null)
                {
                    if (!outpostCounts.ContainsKey(outpost.GetComponent<DrillerCarrier>().GetOwner()))
                        outpostCounts.Add(outpost.GetComponent<DrillerCarrier>().GetOwner(), 1);
                    else
                        outpostCounts[outpost.GetComponent<DrillerCarrier>().GetOwner()]++;
                }
            }

            
            foreach (KeyValuePair<Player, int> keyValuePair in outpostCounts)
            {
                Assert.AreEqual(config.MapConfiguration.OutpostsPerPlayer, keyValuePair.Value);   
            }
        }

        [TestMethod]
        public void UnownedOutpostsHaveNoDrillers()
        {
            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaximumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;

            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            List<Outpost> generatedOutposts = generator.GenerateMap();

            // Loop through all unowned outpost and verify that the driller count is 0 for each
            foreach (Outpost outpost in generatedOutposts.Where(x => x.GetComponent<DrillerCarrier>().GetOwner() == null))
            {
                Assert.AreEqual(0, outpost.GetComponent<DrillerCarrier>().GetDrillerCount());
            }
        }

        [TestMethod]
        public void UnownedOutpostsHaveNoShieldGeneration()
        {
            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaximumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;

            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            List<Outpost> generatedOutposts = generator.GenerateMap();

            // Advance the time forward until next shield production
            _timeMachine.Advance(Constants.BASE_SHIELD_REGENERATION_TICKS);

            // Check that each unowned outpost has not generated shields
            foreach (Outpost outpost in generatedOutposts.Where(x => x.GetComponent<DrillerCarrier>().GetOwner() == null))
            {
                Assert.AreEqual(0, outpost.GetComponent<ShieldManager>().GetShields());
            }
        }

        [TestMethod]
        public void UnownedOutpostsHaveNoDrillerProduction()
        {
            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaximumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;

            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            List<Outpost> generatedOutposts = generator.GenerateMap();

            // Advance the time forward until next production
            _timeMachine.Advance(Constants.TicksPerProduction);

            // Check that each unowned outpost has not produced drillers
            foreach (Outpost outpost in generatedOutposts.Where(x => x.GetComponent<DrillerCarrier>().GetOwner() == null))
            {
                Assert.AreEqual(0, outpost.GetComponent<DrillerCarrier>().GetDrillerCount());
            }
        }

        [TestMethod]
        public void AllPlayersHaveAQueen()
        {
            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaximumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            List<Outpost> generatedOutposts = generator.GenerateMap();

            Dictionary<Player, int> queenCounts = new Dictionary<Player, int>();
            foreach (Outpost outpost in generatedOutposts)
            {
                // Ignore dormant outposts
                if (outpost.GetComponent<DrillerCarrier>().GetOwner() != null && outpost.GetComponent<SpecialistManager>().GetUncapturedSpecialistCount() == 1)
                {
                    if (!queenCounts.ContainsKey(outpost.GetComponent<DrillerCarrier>().GetOwner()))
                        queenCounts.Add(outpost.GetComponent<DrillerCarrier>().GetOwner(), 1);
                    else
                        queenCounts[outpost.GetComponent<DrillerCarrier>().GetOwner()]++;
                }
            }

            
            foreach (KeyValuePair<Player, int> keyValuePair in queenCounts)
            {
                Assert.AreEqual(1, keyValuePair.Value);   
            }
        }

        [TestMethod]
        public void MaxOutpostDistanceRespected()
        {
            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 0;
            config.MapConfiguration.MaximumOutpostDistance = 0;
            config.MapConfiguration.MinimumOutpostDistance = 0;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            // Ensure all 7 outposts were generated on top of each other.
            // Should be at 0,0
            foreach (Outpost o in generatedOutposts)
            {
                Assert.AreEqual(0, o.GetComponent<PositionManager>().CurrentLocation.X);
                Assert.AreEqual(0, o.GetComponent<PositionManager>().CurrentLocation.Y);
            }
        }
        
        [TestMethod]
        public void OutpostsPerPlayerRespected()
        {
            // List<Player> players = new List<Player> { new Player("1") };

            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 0;
            config.MapConfiguration.MaximumOutpostDistance = 0;
            config.MapConfiguration.MinimumOutpostDistance = 0;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            // Ensure all 7 outposts were generated on top of each other.
            // Should be at 0,0
            Assert.AreEqual(config.MapConfiguration.OutpostsPerPlayer * players.Count, generatedOutposts.Count);
        }
        
        [TestMethod]
        [ExpectedException(typeof(PlayerCountException))]
        public void CannotCreateGameWithNoPlayers()
        {
            List<Player> players = new List<Player>();
            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            new MapGenerator(config.MapConfiguration, players, _timeMachine);
            
        }
        
        [TestMethod]
        [ExpectedException(typeof(OutpostPerPlayerException))]
        public void cannotCreateGameWithAtLeastOneOutpostPerPlayer()
        {
            // List<Player> players = new List<Player> { new Player("1") };
            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            config.MapConfiguration.OutpostsPerPlayer = 0;
            new MapGenerator(config.MapConfiguration, players, _timeMachine);
            
        }
        
        [TestMethod]
        public void DormantsPerPlayerRespected()
        {
            // List<Player> players = new List<Player> { new Player("1") };

            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 7;
            config.MapConfiguration.MaximumOutpostDistance = 0;
            config.MapConfiguration.MinimumOutpostDistance = 0;
            config.MapConfiguration.OutpostsPerPlayer = 1;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            // Ensure all 7 outposts were generated
            // Ensure they all have no owner.
            int dormants = 0;
            foreach (Outpost o in generatedOutposts)
            {
                if (o.GetComponent<DrillerCarrier>().GetOwner() == null)
                {
                    dormants++;
                }
            }
            Assert.AreEqual(config.MapConfiguration.DormantsPerPlayer * players.Count, dormants);
        }
        
        [TestMethod]
        public void MinimumOutpostDistanceRespected()
        {
            // List<Player> players = new List<Player> { new Player("1") };

            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 0;
            config.MapConfiguration.MaximumOutpostDistance = 2;
            config.MapConfiguration.MinimumOutpostDistance = 1;
            config.MapConfiguration.OutpostsPerPlayer = 2;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            // Ensure the distance between outposts is over 199.
            Assert.AreEqual(config.MapConfiguration.OutpostsPerPlayer * players.Count, generatedOutposts.Count);
            // Ensure the distance between the two is respected.
            Outpost outpost1 = generatedOutposts[0];
            Outpost outpost2 = generatedOutposts[1];

            float distance = (outpost1.GetComponent<PositionManager>().CurrentLocation-outpost2.GetComponent<PositionManager>().CurrentLocation).Length();
            Assert.IsTrue(distance >= config.MapConfiguration.MinimumOutpostDistance);
        }

        [TestMethod]
        public void AllOutpostsHaveUniqueNames()
        {
            GameConfiguration config = _testUtils.GetDefaultGameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.UtcNow.Millisecond);
            config.MapConfiguration.Seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaximumOutpostDistance = 200;
            config.MapConfiguration.MinimumOutpostDistance = 20;
            config.MapConfiguration.OutpostsPerPlayer = 3;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration, players, _timeMachine);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            Assert.AreEqual(generatedOutposts.Select(x => x.GetComponent<IdentityManager>().GetName()).Distinct().Count(), generatedOutposts.Count);   
        }
        

    }
}