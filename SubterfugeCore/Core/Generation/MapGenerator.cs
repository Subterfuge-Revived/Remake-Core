using System;
using System.Collections.Generic;
using System.Numerics;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Generation
{
    public class MapGenerator
    {
        // Generation parameters
        public int NumPlayers, OutpostsPerPlayer, MinOutpostDistance, MaxSeedDistance, DormantsPerPlayer;
        // List of players in the game
        public List<Player> Players = new List<Player>();
        // List of the generated outposts
        public List<Outpost> Outposts = new List<Outpost>();
        // Seeded Random for number generation
        public SeededRandom RandomGenerator;
        // To generate outpost names.
        public NameGenerator NameGenerator;
        // Seeded Random for number generation
        public Rft map = new Rft(300, 300);

        /// <summary>
        /// Map Generation constructor to seed map generation
        /// </summary>
        /// <param name="seed">The seed to set the random number generator</param>
        public MapGenerator(GameConfiguration gameConfiguration)
        {
            if (gameConfiguration.Players.Count <= 0)
            {
                throw new PlayerCountException("A game must have at least one player.");
            }

            if (gameConfiguration.OutpostsPerPlayer <= 0)
            {
                throw new OutpostPerPlayerException("outpostsPerPlayer must be greater than or equal to one.");
            }
            
            this.RandomGenerator = new SeededRandom(gameConfiguration.Seed);
            this.NameGenerator = new NameGenerator(RandomGenerator);
            this.DormantsPerPlayer = gameConfiguration.DormantsPerPlayer;
            this.Players = gameConfiguration.Players;
            this.NumPlayers = gameConfiguration.Players.Count;
            this.OutpostsPerPlayer = gameConfiguration.OutpostsPerPlayer;
            this.MinOutpostDistance = gameConfiguration.MinimumOutpostDistance;
            this.MaxSeedDistance = gameConfiguration.MaxiumumOutpostDistance;
        }

        /// <summary>
        /// Translates a given set of outposts by a set amount, duplicating the outposts at a different coordinate location.
        /// Result from this function is a newly generated set of outposts offset by the specified translation.
        /// </summary>
        /// <param name="outposts">A list of outposts to translate</param>
        /// <param name="translation">The translation to apply</param>
        /// <returns>A list of translated outposts</returns>
        private List<Outpost> TranslateOutposts(List<Outpost> outposts, RftVector translation)
        {
            // Generate a random rotation of 0/90/180/270 so the map doesn't appear to be tiled.
            double rotation = RandomGenerator.NextRand(0, 3) * Math.PI / 2;

            // List to store the newly translated outposts
            List<Outpost> translatedOutposts = new List<Outpost>();
            
            // Loop through the original outposts.
            foreach(Outpost outpost in outposts)
            {
                // Get original outpost location
                RftVector position = outpost.GetCurrentPosition();
                
                // New vector for the copied location
                RftVector newPosition = new RftVector(outpost.GetCurrentPosition().Map, position.X, position.Y);

                // Undo the rotation and apply a new rotation.
                // https://stackoverflow.com/questions/620745/c-rotating-a-vector-around-a-certain-point
                double cs = Math.Cos(rotation);
                double sn = Math.Sin(rotation);

                double translatedX = position.X;
                double translatedY = position.Y;

                double resultX = translatedX * cs - translatedY * sn;
                double resultY = translatedY * cs - translatedX * sn;

                resultX += this.MaxSeedDistance;
                resultY += this.MaxSeedDistance;

                newPosition.X = (float)resultX;
                newPosition.Y = (float)resultY;
                
                // Apply the translation to offset the outpost.
                newPosition += translation;

                // Add a new outpost to the translated outposts list
                Outpost newOutpost = new Outpost(newPosition, outpost.GetOwner(), outpost.GetOutpostType());
                newOutpost.Name = this.NameGenerator.GetRandomName();
                translatedOutposts.Add(newOutpost);
            }
            // Return the translated outposts.
            return translatedOutposts;
        }

        /// <summary>
        /// Generates a set of outposts for one player based on the map generation configurations.
        /// </summary>
        /// <returns>A list of a single player's outposts</returns>
        public List<Outpost> GeneratePlayerOutposts()
        {
            // List of a player's outposts.
            List<Outpost> playerOutposts = new List<Outpost>();
            
            // setup variables
            double direction;
            float distance;
            bool usableLocation = true;
            int x, y, idx;
            RftVector vectorDistance;
            RftVector currentOutpostPosition;
            Outpost currentOutpost, otherOutpost;

            // Loop to generate outposts until the number of generated outposts is valid
            while (playerOutposts.Count < this.OutpostsPerPlayer + this.DormantsPerPlayer)
            {
                // calculate the new outposts location within allowable raidius
                distance = (float)(this.RandomGenerator.NextDouble() * (MaxSeedDistance - MinOutpostDistance)) + MinOutpostDistance;
                direction = this.RandomGenerator.NextDouble() * Math.PI * 2;  // In radians
                
                // Determine the type of outpost that is generated
                OutpostType type = (OutpostType)this.RandomGenerator.NextRand(0, 5);
                
                //convert distance & direction into vector X and Y
                x = Convert.ToInt32(Math.Cos(direction) * distance);
                y = Convert.ToInt32(Math.Sin(direction) * distance);
                currentOutpostPosition = new RftVector(map, x, y);

                usableLocation = true;
                // Determine if the generated location is too close to another outpost
                for (idx = 0; idx < playerOutposts.Count & usableLocation; idx++)
                {
                    // Get the X and Y pos to find distance
                    otherOutpost = playerOutposts[idx];
                    vectorDistance = otherOutpost.GetPosition() - currentOutpostPosition;

                    //ensure that the new location is not too close to other outposts
                    if (vectorDistance.Magnitude() < MinOutpostDistance)
                    {
                        usableLocation = false;
                    }
                }
                
                // If the location is not too close to another outpost, add the outpost to the list.
                if (usableLocation || playerOutposts.Count == 0)
                {
                    currentOutpost = new Outpost(currentOutpostPosition, type);
                    currentOutpost.Name = this.NameGenerator.GetRandomName();
                    playerOutposts.Add(currentOutpost);
                }
            }
            
            // Return list of generated outposts.
            return playerOutposts;
        }
        
        /// <summary>
        ///  From a list of generated outposts, sets the X closest outposts to the centroid to be owned by the specified player.
        ///  Remaining outposts are dormant.
        /// </summary>
        /// <param name="outposts">The list of outposts to be owned</param>
        /// <param name="player">The player to own the outposts</param>
        public void SetOutpostOwner(List<Outpost> outposts, Player player)
        {
            // A list of the outposts that are closest to the centroid of the player's land.
            // This list is the list of outposts that the player should own.
            List<Outpost> closestOutposts = new List<Outpost>();
            
            // The centroid
            RftVector centroid = new RftVector(map, 0, 0);

            // Loop outposts in the generation
            foreach (Outpost o in outposts)
            {
                // Add outposts to the closestOutposts list until it has "outpostsPerPlayer" outposts
                if(closestOutposts.Count < this.OutpostsPerPlayer)
                {
                    closestOutposts.Add(o);
                } else {
                    // Determine the distance to the current outpost from the centroid
                    float currentDistance = (centroid - o.GetCurrentPosition()).Magnitude();
                    
                    // Sort the closestOutpost list to determine the farthest outpost (maybe this one is closer).
                    closestOutposts.Sort((a, b) => (int)((centroid - a.GetCurrentPosition()).Magnitude() - (centroid - b.GetCurrentPosition()).Magnitude()));
                    
                    // Determine the distance of the farthest outpost
                    float farthestDistance = (centroid - closestOutposts[this.OutpostsPerPlayer - 1].GetCurrentPosition()).Magnitude();

                    // If the current outpost is closer, put the current outpost in the list, replacing the farther outpost.
                    if(currentDistance < farthestDistance)
                    {
                        closestOutposts[this.OutpostsPerPlayer - 1] = o;
                    }
                }
            }
            
            // Once the closest outposts are determined, set the owner.
            // And provide some drillers.
            foreach(Outpost closeOutposts in closestOutposts)
            {
                closeOutposts.SetOwner(player);
                closeOutposts.AddDrillers(30);
            }
        }

        /// <summary>
        /// Generates the map
        /// </summary>
        /// <returns>A list of outposts within the map</returns>
        public List<Outpost> GenerateMap()
        {
            // To generate the map we will:
            // 1. Generate a single player's outposts
            // 2. Translate the outposts for each player in the game
            // 3. Set the ownership
            
            
            // Generate a set of a single player's outposts:
            List<Outpost> firstPlayerOutposts = this.GeneratePlayerOutposts();
            
            // Set the ownership of the central X generated outposts to the first player.
            if (OutpostsPerPlayer > 0 && Players.Count > 0)
                this.SetOutpostOwner(firstPlayerOutposts, Players[0]);

            // Tile the outposts based on the # of players on a 2xn grid
            // Example:
            //
            // 1 3 5 7 9
            // 2 4 6 8 10
            //
            // Each # is a player, each player has a (2 * maxSeedDistance)x(2 * maxSeedDistance) area
            int width = (int)(Math.Ceiling(this.NumPlayers / 2.0f)); // calculate n
            int height = 2;

            // Counter for looping through all players
            int widthCounter = 1;
            int heightCounter = 1;

            // Count # of generations
            int playersGenerated = 0;

            // Loop from left to right of player grid
            while (widthCounter <= width)
            {
                // Loop from top to bottom of player grid
                while (heightCounter <= height)
                {
                    // Stop early if there is an odd number of players. Ex. w/ 7 players we don't want to generate the 8th grid.
                    if (playersGenerated < this.Players.Count)
                    {
                        // Translate the first player's outposts based on the width/height counter for this player.
                        // Function also applies a random rotation.
                        List<Outpost> translatedOutposts = this.TranslateOutposts(firstPlayerOutposts, new RftVector(map, this.MaxSeedDistance * widthCounter * 2, this.MaxSeedDistance * heightCounter * 2));

                        // Update the owner to the new player
                        bool queenGenerated = false;
                        foreach (Outpost o in translatedOutposts)
                        {
                            if (!queenGenerated)
                            {
                                o.GetSpecialistManager().AddSpecialist(new Queen(o.GetOwner()));
                                queenGenerated = true;
                            }
                            if(o.GetOwner() != null)
                                o.SetOwner(Players[(widthCounter - 1) * 2 + (heightCounter - 1)]);
                        }

                        // Add the outposts to the generated list.
                        this.Outposts.AddRange(translatedOutposts);
                        
                        // Increment the map generation loop
                        heightCounter++;
                        playersGenerated++;
                    } else
                    {
                        // Return the outposts if we stop early.
                        return this.Outposts;
                    }
                }
                // After a height of 2, reset the height to 1 but increase the width
                widthCounter++;
                heightCounter = 1;
            }

            // Return a list of the generated outposts.
            return this.Outposts;
        }
    }

    public class PlayerCountException : Exception
    {
        public PlayerCountException(string exception) : base(exception)
        {
            
        }
    }

    public class OutpostPerPlayerException : Exception
    {
        public OutpostPerPlayerException(string message) : base(message)
        {
            
        }
    }
}
