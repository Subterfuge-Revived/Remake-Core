using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Entities;
using Microsoft.Xna.Framework;
using static SubterfugeCore.GameServer;
using SubterfugeCore.Timing;
using SubterfugeCore.Players;

namespace SubterfugeCore
{
    public class MapGenerator
    {
        public int numPlayers, outpostsPerPlayer, minOutpostDistance, maxSeedDistance, dormantsPerPlayer;
        public List<Outpost> Outposts = new List<Outpost>();
        public SeededRandom randomGenerator;

        // Constructor takes in seed so that it generates the same set of outposts each time
        public MapGenerator(int seed)
        {
            this.randomGenerator = new SeededRandom(seed);
        }

        public MapGenerator()
        {
        }

        public void SetData(int numPlayersPram, int outpostsPerPlayerPram, int dormantsPerPlayer, int minOutpostDistancePram, int maxSeedDistancePram)
        {
            this.dormantsPerPlayer = dormantsPerPlayer;
            this.numPlayers = numPlayersPram;
            this.outpostsPerPlayer = outpostsPerPlayerPram;
            this.minOutpostDistance = minOutpostDistancePram;
            this.maxSeedDistance = maxSeedDistancePram;
        }

        public List<Outpost> translateOutposts(List<Outpost> outposts, Vector2 translation)
        {
            // Generate a random rotation of 0/90/180/270 so the map doesn't appear to be tiled.
            double rotation = randomGenerator.nextRand(0, 3) * Math.PI / 2;

            List<Outpost> translatedOutposts = new List<Outpost>();
            foreach(Outpost outpost in outposts)
            {
                Vector2 position = outpost.getCurrentLocation();
                Vector2 newPosition = new Vector2(position.X, position.Y);
                float magnitude = position.Length();

                // Undo the rotation and apply a new rotation.
                // https://stackoverflow.com/questions/620745/c-rotating-a-vector-around-a-certain-point
                double cs = Math.Cos(rotation);
                double sn = Math.Sin(rotation);

                double translated_x = position.X;
                double translated_y = position.Y;

                double result_x = translated_x * cs - translated_y * sn;
                double result_y = translated_y * cs - translated_x * sn;

                result_x += this.maxSeedDistance;
                result_y += this.maxSeedDistance;

                newPosition.X = (float)result_x;
                newPosition.Y = (float)result_y;
                newPosition += translation;

                translatedOutposts.Add(new Outpost(newPosition, outpost.getOwner()));
            }
            return translatedOutposts;
        }

        // Generates a single player's outposts.
        public List<Outpost> generatePlayerOutposts()
        {
            List<Outpost> playerOutposts = new List<Outpost>();
            // setup variables
            double direction;
            int distance;
            bool usableLocation = true;
            int x, y, IDX;
            Vector2 vectorDistance;
            Vector2 currentOutpostLocation;
            Outpost currentOutpost, otherOutpost;

            while (playerOutposts.Count < this.outpostsPerPlayer + this.dormantsPerPlayer)
            {
                // calculate the new outposts location within allowable raidius
                distance = this.randomGenerator.nextRand(minOutpostDistance, maxSeedDistance);
                direction = this.randomGenerator.nextDouble() * Math.PI * 2;
                //convert vector into X and Y
                x = Convert.ToInt32(Math.Cos(direction) * distance);
                y = Convert.ToInt32(Math.Sin(direction) * distance);
                currentOutpostLocation.X = x;
                currentOutpostLocation.Y = y;

                usableLocation = true;
                for (IDX = 0; IDX < playerOutposts.Count & usableLocation; IDX++)
                {
                    // Get the X and Y pos to find distance
                    otherOutpost = playerOutposts[IDX];
                    vectorDistance = otherOutpost.getPosition() - currentOutpostLocation;

                    //ensure that the new location is not too close to other outposts
                    if (vectorDistance.Length() < minOutpostDistance)
                    {
                        usableLocation = false;
                    }


                }
                if (usableLocation || playerOutposts.Count == 0)
                {
                    currentOutpost = new Outpost(currentOutpostLocation);
                    playerOutposts.Add(currentOutpost);
                }
            }
            return playerOutposts;
        }

        public void setOutpostOwner(List<Outpost> outposts, Player player)
        {
            // Get the centroid of the outposts, determine the closest outposts and set their owners
            List<Outpost> closestOutposts = new List<Outpost>();
            Vector2 centroid = new Vector2(0, 0);

            foreach (Outpost o in outposts)
            {
                // Get the closest outpostsPerPlayer of outposts and make the player their owner.
                if(closestOutposts.Count < this.outpostsPerPlayer)
                {
                    closestOutposts.Add(o);
                    continue;
                } else
                {
                    float currentDistance = (centroid - o.getCurrentLocation()).Length();
                    // Determine the shortest distance.
                    closestOutposts.Sort((a, b) => (int)((centroid - a.getCurrentLocation()).Length() - (centroid - b.getCurrentLocation()).Length()));
                    float farthestDistance = (centroid - closestOutposts[4].getCurrentLocation()).Length();

                    if(currentDistance < farthestDistance)
                    {
                        closestOutposts[4] = o;
                    }
                }
            }

            foreach(Outpost closeOutposts in closestOutposts)
            {
                closeOutposts.setOwner(player);
            }
        }

        public int getPlayerOutposts(Player player)
        {
            return this.Outposts.FindAll((o) => o.getOwner() == player).Count;
        }

        public List<Outpost> GenerateMap()
        {
            // Generate a set of a single player's outposts:
            List<Outpost> firstPlayerOutposts = this.generatePlayerOutposts();
            this.setOutpostOwner(firstPlayerOutposts, GameServer.timeMachine.getState().getPlayers()[0]);

            // Tile the outposts based on the # of players on a 2xn grid
            int width = (int)(Math.Ceiling(this.numPlayers / 2.0f));
            int height = 2;

            // Each tile's size is a square of maxSeedDistance x maxSeedDistance
            int widthCounter = 1;
            int heightCounter = 1;

            int playersGenerated = 0;

            while (widthCounter <= width)
            {
                while (heightCounter <= height)
                {
                    if (playersGenerated < this.numPlayers)
                    {
                        // Translate down
                        List<Outpost> translatedOutposts = this.translateOutposts(firstPlayerOutposts, new Vector2(this.maxSeedDistance * widthCounter * 2, this.maxSeedDistance * heightCounter * 2));


                        foreach (Outpost o in translatedOutposts)
                        {
                            if(o.getOwner() != null)
                                o.setOwner(GameServer.timeMachine.getState().getPlayers()[(widthCounter - 1) * 2 + (heightCounter - 1)]);
                        }

                        // Add the outposts to the generated list.
                        this.Outposts.AddRange(translatedOutposts);
                        heightCounter++;
                        playersGenerated++;
                    } else
                    {
                        return this.Outposts;
                    }
                }
                widthCounter++;
                heightCounter = 1;
            }

            return this.Outposts;

        }
    }
}
