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
            config.dormantsPerPlayer = 3;
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
            
            // Add one for dormant outposts.
            Assert.AreEqual(config.players.Count + 1, playersGenerated.Count);
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

    }
}
