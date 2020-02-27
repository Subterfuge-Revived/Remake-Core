using System;
using System.Collections.Generic;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class MapGeneratorTest
    {


        [TestMethod]
        public void GameConfigurationConstructor()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            players.Add(new Player(2));
            players.Add(new Player(3));
            players.Add(new Player(4));
            
            Assert.IsNotNull(new GameConfiguration(players));
        }
        
        [TestMethod]
        public void GameConfigurationSettersAndGetters()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            players.Add(new Player(2));
            players.Add(new Player(3));
            players.Add(new Player(4));
            
            GameConfiguration config = new GameConfiguration(players);
            
            int seed = 1234;
            config.seed = seed;
            Assert.AreEqual(seed, config.seed);
            
            int dormantsPerPlayer = 3;
            config.dormantsPerPlayer = dormantsPerPlayer;
            Assert.AreEqual(dormantsPerPlayer, config.dormantsPerPlayer);
            
            int maxiumumOutpostDistance = 100;
            config.maxiumumOutpostDistance = maxiumumOutpostDistance;
            Assert.AreEqual(maxiumumOutpostDistance, config.maxiumumOutpostDistance);
            
            int minimumOutpostDistance = 5;
            config.minimumOutpostDistance = minimumOutpostDistance;
            Assert.AreEqual(minimumOutpostDistance, config.minimumOutpostDistance);
            
            int outpostsPerPlayer = 7;
            config.outpostsPerPlayer = outpostsPerPlayer;
            Assert.AreEqual(outpostsPerPlayer, config.outpostsPerPlayer);
        }
        
        [TestMethod]
        public void constructor()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            players.Add(new Player(2));
            players.Add(new Player(3));
            players.Add(new Player(4));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            config.seed = 1234;
            config.dormantsPerPlayer = 3;
            config.maxiumumOutpostDistance = 100;
            config.minimumOutpostDistance = 5;
            config.outpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config);
            Assert.IsNotNull(generator);
        }

        [TestMethod]
        public void generatesTheRightNumberOfOutposts()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            players.Add(new Player(2));
            players.Add(new Player(3));
            players.Add(new Player(4));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            config.seed = 1234;
            config.dormantsPerPlayer = 3;
            config.maxiumumOutpostDistance = 100;
            config.minimumOutpostDistance = 5;
            config.outpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            Assert.AreEqual(config.players.Count * (config.outpostsPerPlayer + config.dormantsPerPlayer), generatedOutposts.Count);
        }
        
        [TestMethod]
        public void GeneratesOutpostsForAllPlayers()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            players.Add(new Player(2));
            players.Add(new Player(3));
            players.Add(new Player(4));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            config.seed = 1234;
            config.dormantsPerPlayer = 0;
            config.maxiumumOutpostDistance = 100;
            config.minimumOutpostDistance = 5;
            config.outpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config);
            List<Outpost> generatedOutposts = generator.GenerateMap();

            List<Player> playersGenerated = new List<Player>();
            foreach (Outpost outpost in generatedOutposts)
            {
                if(!playersGenerated.Contains(outpost.getOwner()))
                    playersGenerated.Add(outpost.getOwner());
            }
            
            Assert.AreEqual(config.players.Count, playersGenerated.Count);
        }
        
        [TestMethod]
        public void GeneratesEqualOutpostsForAllPlayers()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            players.Add(new Player(2));
            players.Add(new Player(3));
            players.Add(new Player(4));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            config.seed = 1234;
            config.dormantsPerPlayer = 3;
            config.maxiumumOutpostDistance = 100;
            config.minimumOutpostDistance = 5;
            config.outpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config);
            List<Outpost> generatedOutposts = generator.GenerateMap();

            Dictionary<Player, int> outpostCounts = new Dictionary<Player, int>();
            foreach (Outpost outpost in generatedOutposts)
            {
                // Ignore dormant outposts
                if (outpost.getOwner() != null)
                {
                    if (!outpostCounts.ContainsKey(outpost.getOwner()))
                        outpostCounts.Add(outpost.getOwner(), 1);
                    else
                        outpostCounts[outpost.getOwner()]++;
                }
            }

            
            foreach (KeyValuePair<Player, int> keyValuePair in outpostCounts)
            {
                Assert.AreEqual(config.outpostsPerPlayer, keyValuePair.Value);   
            }
        }
        
        [TestMethod]
        public void AllPlayersHaveAQueen()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            players.Add(new Player(2));
            players.Add(new Player(3));
            players.Add(new Player(4));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            config.seed = 1234;
            config.dormantsPerPlayer = 3;
            config.maxiumumOutpostDistance = 100;
            config.minimumOutpostDistance = 5;
            config.outpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config);
            List<Outpost> generatedOutposts = generator.GenerateMap();

            Dictionary<Player, int> queenCounts = new Dictionary<Player, int>();
            foreach (Outpost outpost in generatedOutposts)
            {
                // Ignore dormant outposts
                if (outpost.getOwner() != null && outpost.getSpecialistManager().getSpecialistCount() == 1)
                {
                    if (!queenCounts.ContainsKey(outpost.getOwner()))
                        queenCounts.Add(outpost.getOwner(), 1);
                    else
                        queenCounts[outpost.getOwner()]++;
                }
            }

            
            foreach (KeyValuePair<Player, int> keyValuePair in queenCounts)
            {
                Assert.AreEqual(1, keyValuePair.Value);   
            }
        }

        [TestMethod]
        public void maxOutpostDistanceRespected()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            config.seed = 1234;
            config.dormantsPerPlayer = 0;
            config.maxiumumOutpostDistance = 0;
            config.minimumOutpostDistance = 0;
            config.outpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            // Ensure all 7 outposts were generated on top of each other.
            // Should be at 0,0
            foreach (Outpost o in generatedOutposts)
            {
                Assert.AreEqual(0, o.getCurrentLocation().X);
                Assert.AreEqual(0, o.getCurrentLocation().Y);
            }
        }
        
        [TestMethod]
        public void outpostsPerPlayerRespected()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            config.seed = 1234;
            config.dormantsPerPlayer = 0;
            config.maxiumumOutpostDistance = 0;
            config.minimumOutpostDistance = 0;
            config.outpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            // Ensure all 7 outposts were generated on top of each other.
            // Should be at 0,0
            Assert.AreEqual(config.outpostsPerPlayer, generatedOutposts.Count);
        }
        
        [TestMethod]
        [ExpectedException(typeof(PlayerCountException))]
        public void cannotCreateGameWithNoPlayers()
        {
            List<Player> players = new List<Player>();
            GameConfiguration config = new GameConfiguration(players);
            MapGenerator generator = new MapGenerator(config);
            
        }
        
        [TestMethod]
        [ExpectedException(typeof(OutpostPerPlayerException))]
        public void cannotCreateGameWithAtLeastOneOutpostPerPlayer()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            GameConfiguration config = new GameConfiguration(players);
            config.outpostsPerPlayer = 0;
            MapGenerator generator = new MapGenerator(config);
            
        }
        
        [TestMethod]
        public void dormantsPerPlayerRespected()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            config.seed = 1234;
            config.dormantsPerPlayer = 7;
            config.maxiumumOutpostDistance = 0;
            config.minimumOutpostDistance = 0;
            config.outpostsPerPlayer = 1;
            
            MapGenerator generator = new MapGenerator(config);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            // Ensure all 7 outposts were generated
            // Ensure they all have no owner.
            int dormants = 0;
            foreach (Outpost o in generatedOutposts)
            {
                if (o.getOwner() == null)
                {
                    dormants++;
                }
            }
            Assert.AreEqual(config.dormantsPerPlayer, dormants);
        }
        
        [TestMethod]
        public void minimumOutpostDistanceRespected()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player(1));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            config.seed = 1234;
            config.dormantsPerPlayer = 0;
            config.maxiumumOutpostDistance = 2;
            config.minimumOutpostDistance = 1;
            config.outpostsPerPlayer = 2;
            
            MapGenerator generator = new MapGenerator(config);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            // Ensure the distance between outposts is over 199.
            Assert.AreEqual(2, generatedOutposts.Count);
            // Ensure the distance between the two is respected.
            Outpost outpost1 = generatedOutposts[0];
            Outpost outpost2 = generatedOutposts[1];

            float distance = Vector2.Distance(outpost1.getCurrentLocation(), outpost2.getCurrentLocation());
            Assert.IsTrue(distance > config.minimumOutpostDistance);
        }
        

    }
}
