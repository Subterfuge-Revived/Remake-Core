﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubterfugeCore;
using SubterfugeCore.Core;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCoreTest
{
    [TestClass]
    public class MapGeneratorTest
    {


        [TestMethod]
        public void GameConfigurationConstructor()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            players.Add(new Player("2"));
            players.Add(new Player("3"));
            players.Add(new Player("4"));
            
            Assert.IsNotNull(new GameConfiguration(players));
        }
        
        [TestMethod]
        public void GameConfigurationSettersAndGetters()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            players.Add(new Player("2"));
            players.Add(new Player("3"));
            players.Add(new Player("4"));
            
            GameConfiguration config = new GameConfiguration(players);
            
            Random rand = new Random(DateTime.Now.Millisecond);
            int seed = rand.Next();
            config.MapConfiguration.Seed = seed;
            Assert.AreEqual(seed, config.MapConfiguration.Seed);
            
            int dormantsPerPlayer = 3;
            config.MapConfiguration.DormantsPerPlayer = dormantsPerPlayer;
            Assert.AreEqual(dormantsPerPlayer, config.MapConfiguration.DormantsPerPlayer);
            
            int maxiumumOutpostDistance = 100;
            config.MapConfiguration.MaxiumumOutpostDistance = maxiumumOutpostDistance;
            Assert.AreEqual(maxiumumOutpostDistance, config.MapConfiguration.MaxiumumOutpostDistance);
            
            int minimumOutpostDistance = 5;
            config.MapConfiguration.MinimumOutpostDistance = minimumOutpostDistance;
            Assert.AreEqual(minimumOutpostDistance, config.MapConfiguration.MinimumOutpostDistance);
            
            int outpostsPerPlayer = 7;
            config.MapConfiguration.OutpostsPerPlayer = outpostsPerPlayer;
            Assert.AreEqual(outpostsPerPlayer, config.MapConfiguration.OutpostsPerPlayer);
        }
        
        [TestMethod]
        public void Constructor()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            players.Add(new Player("2"));
            players.Add(new Player("3"));
            players.Add(new Player("4"));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.Now.Millisecond);
            int seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaxiumumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration);
            Assert.IsNotNull(generator);
        }

        [TestMethod]
        public void GeneratesTheRightNumberOfOutposts()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            players.Add(new Player("2"));
            players.Add(new Player("3"));
            players.Add(new Player("4"));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.Now.Millisecond);
            int seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaxiumumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            Assert.AreEqual(config.Players.Count * (config.MapConfiguration.OutpostsPerPlayer + config.MapConfiguration.DormantsPerPlayer), generatedOutposts.Count);
        }
        
        [TestMethod]
        public void GeneratesOutpostsForAllPlayers()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            players.Add(new Player("2"));
            players.Add(new Player("3"));
            players.Add(new Player("4"));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.Now.Millisecond);
            int seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 0;
            config.MapConfiguration.MaxiumumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration);
            List<Outpost> generatedOutposts = generator.GenerateMap();

            List<Player> playersGenerated = new List<Player>();
            foreach (Outpost outpost in generatedOutposts)
            {
                if(!playersGenerated.Contains(outpost.GetOwner()))
                    playersGenerated.Add(outpost.GetOwner());
            }
            
            Assert.AreEqual(config.Players.Count, playersGenerated.Count);
        }
        
        [TestMethod]
        public void GeneratesEqualOutpostsForAllPlayers()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            players.Add(new Player("2"));
            players.Add(new Player("3"));
            players.Add(new Player("4"));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.Now.Millisecond);
            int seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaxiumumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration);
            List<Outpost> generatedOutposts = generator.GenerateMap();

            Dictionary<Player, int> outpostCounts = new Dictionary<Player, int>();
            foreach (Outpost outpost in generatedOutposts)
            {
                // Ignore dormant outposts
                if (outpost.GetOwner() != null)
                {
                    if (!outpostCounts.ContainsKey(outpost.GetOwner()))
                        outpostCounts.Add(outpost.GetOwner(), 1);
                    else
                        outpostCounts[outpost.GetOwner()]++;
                }
            }

            
            foreach (KeyValuePair<Player, int> keyValuePair in outpostCounts)
            {
                Assert.AreEqual(config.MapConfiguration.OutpostsPerPlayer, keyValuePair.Value);   
            }
        }
        
        [TestMethod]
        public void AllPlayersHaveAQueen()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            players.Add(new Player("2"));
            players.Add(new Player("3"));
            players.Add(new Player("4"));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.Now.Millisecond);
            int seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaxiumumOutpostDistance = 100;
            config.MapConfiguration.MinimumOutpostDistance = 5;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration);
            List<Outpost> generatedOutposts = generator.GenerateMap();

            Dictionary<Player, int> queenCounts = new Dictionary<Player, int>();
            foreach (Outpost outpost in generatedOutposts)
            {
                // Ignore dormant outposts
                if (outpost.GetOwner() != null && outpost.GetSpecialistManager().GetSpecialistCount() == 1)
                {
                    if (!queenCounts.ContainsKey(outpost.GetOwner()))
                        queenCounts.Add(outpost.GetOwner(), 1);
                    else
                        queenCounts[outpost.GetOwner()]++;
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
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.Now.Millisecond);
            int seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 0;
            config.MapConfiguration.MaxiumumOutpostDistance = 0;
            config.MapConfiguration.MinimumOutpostDistance = 0;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            // Ensure all 7 outposts were generated on top of each other.
            // Should be at 0,0
            foreach (Outpost o in generatedOutposts)
            {
                Assert.AreEqual(0, o.GetCurrentPosition().X);
                Assert.AreEqual(0, o.GetCurrentPosition().Y);
            }
        }
        
        [TestMethod]
        public void OutpostsPerPlayerRespected()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.Now.Millisecond);
            int seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 0;
            config.MapConfiguration.MaxiumumOutpostDistance = 0;
            config.MapConfiguration.MinimumOutpostDistance = 0;
            config.MapConfiguration.OutpostsPerPlayer = 7;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            // Ensure all 7 outposts were generated on top of each other.
            // Should be at 0,0
            Assert.AreEqual(config.MapConfiguration.OutpostsPerPlayer, generatedOutposts.Count);
        }
        
        [TestMethod]
        [ExpectedException(typeof(PlayerCountException))]
        public void CannotCreateGameWithNoPlayers()
        {
            List<Player> players = new List<Player>();
            GameConfiguration config = new GameConfiguration(players);
            MapGenerator generator = new MapGenerator(config.MapConfiguration);
            
        }
        
        [TestMethod]
        [ExpectedException(typeof(OutpostPerPlayerException))]
        public void cannotCreateGameWithAtLeastOneOutpostPerPlayer()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            GameConfiguration config = new GameConfiguration(players);
            config.MapConfiguration.OutpostsPerPlayer = 0;
            MapGenerator generator = new MapGenerator(config.MapConfiguration);
            
        }
        
        [TestMethod]
        public void DormantsPerPlayerRespected()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.Now.Millisecond);
            int seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 7;
            config.MapConfiguration.MaxiumumOutpostDistance = 0;
            config.MapConfiguration.MinimumOutpostDistance = 0;
            config.MapConfiguration.OutpostsPerPlayer = 1;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            // Ensure all 7 outposts were generated
            // Ensure they all have no owner.
            int dormants = 0;
            foreach (Outpost o in generatedOutposts)
            {
                if (o.GetOwner() == null)
                {
                    dormants++;
                }
            }
            Assert.AreEqual(config.MapConfiguration.DormantsPerPlayer, dormants);
        }
        
        [TestMethod]
        public void MinimumOutpostDistanceRespected()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.Now.Millisecond);
            int seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 0;
            config.MapConfiguration.MaxiumumOutpostDistance = 2;
            config.MapConfiguration.MinimumOutpostDistance = 1;
            config.MapConfiguration.OutpostsPerPlayer = 2;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            // Ensure the distance between outposts is over 199.
            Assert.AreEqual(2, generatedOutposts.Count);
            // Ensure the distance between the two is respected.
            Outpost outpost1 = generatedOutposts[0];
            Outpost outpost2 = generatedOutposts[1];

            float distance = outpost1.GetCurrentPosition().Distance(outpost2.GetCurrentPosition());
            Assert.IsTrue(distance >= config.MapConfiguration.MinimumOutpostDistance);
        }

        [TestMethod]
        public void AllOutpostsHaveUniqueNames()
        {
            List<Player> players = new List<Player>();
            players.Add(new Player("1"));
            players.Add(new Player("2"));
            players.Add(new Player("3"));
            
            GameConfiguration config = new GameConfiguration(players);
            Assert.IsNotNull(config);
            Random rand = new Random(DateTime.Now.Millisecond);
            int seed = rand.Next();
            config.MapConfiguration.DormantsPerPlayer = 3;
            config.MapConfiguration.MaxiumumOutpostDistance = 200;
            config.MapConfiguration.MinimumOutpostDistance = 20;
            config.MapConfiguration.OutpostsPerPlayer = 3;
            
            MapGenerator generator = new MapGenerator(config.MapConfiguration);
            List<Outpost> generatedOutposts = generator.GenerateMap();
            
            Assert.AreEqual(generatedOutposts.Select(x => x.Name).Distinct().Count(), generatedOutposts.Count);   
        }
        

    }
}
