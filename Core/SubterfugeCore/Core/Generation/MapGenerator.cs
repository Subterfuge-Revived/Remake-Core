using System;
using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Core.Generation
{
    public class MapGenerator
    {

        /// <summary>
        /// A list of the generated outposts.
        /// </summary>
        private readonly List<Outpost> _outposts = new List<Outpost>();

        /// <summary>
        /// Seeded Random for number generation
        /// </summary>
        private readonly SeededRandom _randomGenerator;

        /// <summary>
        /// To generate outpost names
        /// </summary>
        private readonly NameGenerator _nameGenerator;

        /// <summary>
        /// Rft for map wrapping
        /// </summary>
        private readonly Rft _map = new Rft(300, 300);

        /// <summary>
        /// The generation configuration object.
        /// </summary>
        private readonly MapConfiguration _mapConfiguration;

        /// <summary>
        /// Id Generator for outposts.
        /// </summary>
        private readonly IdGenerator _generator = new IdGenerator();
        
        private readonly List<Player> _players;

        /// <summary>
        /// Map Generation constructor to seed map generation
        /// </summary>
        /// <param name="mapConfiguration">The map configuration parameters</param>
        /// <param name="players">The players in the game</param>
        public MapGenerator(MapConfiguration mapConfiguration, List<Player> players)
        {
            this._players = players;
            this._mapConfiguration = mapConfiguration;
            
            if (players.Count <= 0)
            {
                throw new PlayerCountException("A game must have at least one player.");
            }

            if (mapConfiguration.OutpostsPerPlayer <= 0)
            {
                throw new OutpostPerPlayerException("outpostsPerPlayer must be greater than or equal to one.");
            }

            this._randomGenerator = new SeededRandom(mapConfiguration.Seed);
            this._nameGenerator = new NameGenerator(_randomGenerator);

            // Set the map size.
            int halfPlayers = (int)(Math.Floor(players.Count / 2.0));
            RftVector.Map = new Rft(mapConfiguration.MaximumOutpostDistance * 4, halfPlayers * mapConfiguration.MaximumOutpostDistance * 2);
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
            double rotation = _randomGenerator.NextRand(0, 3) * Math.PI / 2;

            // List to store the newly translated outposts
            List<Outpost> translatedOutposts = new List<Outpost>();

            // Loop through the original outposts.
            foreach (Outpost outpost in outposts)
            {
                // Get original outpost location
                RftVector position = outpost.GetComponent<PositionManager>().GetPositionAt(new GameTick(0));

                // New vector for the copied location
                RftVector newPosition = new RftVector(RftVector.Map, position.X, position.Y);

                // Undo the rotation and apply a new rotation.
                // https://stackoverflow.com/questions/620745/c-rotating-a-vector-around-a-certain-point
                double cs = Math.Cos(rotation);
                double sn = Math.Sin(rotation);

                double translatedX = position.X;
                double translatedY = position.Y;

                double resultX = translatedX * cs - translatedY * sn;
                double resultY = translatedY * cs - translatedX * sn;

                resultX += _mapConfiguration.MaximumOutpostDistance;
                resultY += _mapConfiguration.MaximumOutpostDistance;

                newPosition.X = (float)resultX;
                newPosition.Y = (float)resultY;

                // Apply the translation to offset the outpost.
                newPosition += translation;

                // Add a new outpost to the translated outposts list
                Outpost newOutpost = CreateOutpost(newPosition, outpost.GetOutpostType());
                newOutpost.GetComponent<DrillerCarrier>().SetOwner(outpost.GetComponent<DrillerCarrier>().GetOwner());
                newOutpost.GetComponent<IdentityManager>().SetName(_nameGenerator.GetRandomName());
                translatedOutposts.Add(newOutpost);
            }
            // Return the translated outposts.
            return translatedOutposts;
        }

        /// <summary>
        /// Generates a set of outposts for one player based on the map generation configurations.
        /// </summary>
        /// <returns>A list of a single player's outposts</returns>
        private List<Outpost> GeneratePlayerOutposts()
        {
            // List of a player's outposts.
            List<Outpost> playerOutposts = new List<Outpost>();

            // Loop to generate outposts until the number of generated outposts is valid
            while (playerOutposts.Count < _mapConfiguration.OutpostsPerPlayer + _mapConfiguration.DormantsPerPlayer)
            {
                // calculate the new outposts location within allowable radius
                var distance = this._randomGenerator.NextDouble() * (_mapConfiguration.MaximumOutpostDistance - _mapConfiguration.MinimumOutpostDistance) + _mapConfiguration.MinimumOutpostDistance;
                var direction = this._randomGenerator.NextDouble() * Math.PI * 2;

                // Determine the type of outpost that is generated
                OutpostType[] validTypes =
                {
                    OutpostType.Factory,
                    OutpostType.Generator,
                    OutpostType.Watchtower,
                };
                OutpostType type = validTypes[this._randomGenerator.NextRand(0, 3)];

                //convert distance & direction into vector X and Y
                var x = Convert.ToInt32(Math.Cos(direction) * distance);
                var y = Convert.ToInt32(Math.Sin(direction) * distance);
                var currentOutpostPosition = new RftVector(_map, x, y);

                var usableLocation = true;
                // Determine if the generated location is too close to another outpost
                int idx;
                for (idx = 0; idx < playerOutposts.Count & usableLocation; idx++)
                {
                    // Get the X and Y pos to find distance
                    var otherOutpost = playerOutposts[idx];
                    var vectorDistance = otherOutpost.GetComponent<PositionManager>().GetPositionAt(new GameTick(0)) - currentOutpostPosition;

                    //ensure that the new location is not too close to other outposts
                    if (vectorDistance.Magnitude() < _mapConfiguration.MinimumOutpostDistance)
                    {
                        usableLocation = false;
                    }
                }

                // If the location is not too close to another outpost, add the outpost to the list.
                if (usableLocation || playerOutposts.Count == 0)
                {
                    var currentOutpost = CreateOutpost(currentOutpostPosition, type);
                    currentOutpost.GetComponent<IdentityManager>().SetName(_nameGenerator.GetRandomName());
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
        private void SetOutpostOwner(List<Outpost> outposts, Player player)
        {
            // A list of the outposts that are closest to the centroid of the player's land.
            // This list is the list of outposts that the player should own.
            List<Outpost> closestOutposts = new List<Outpost>();

            // The centroid
            RftVector centroid = new RftVector(_map, 0, 0);

            // Loop outposts in the generation
            foreach (Outpost o in outposts)
            {
                // Add outposts to the closestOutposts list until it has "outpostsPerPlayer" outposts
                if (closestOutposts.Count < _mapConfiguration.OutpostsPerPlayer)
                {
                    closestOutposts.Add(o);
                }
                else
                {
                    // Determine the distance to the current outpost from the centroid
                    float currentDistance = (centroid - o.GetComponent<PositionManager>().GetPositionAt(new GameTick(0))).Magnitude();

                    // Sort the closestOutpost list to determine the farthest outpost (maybe this one is closer).
                    closestOutposts.Sort((a, b) => (int)((centroid - a.GetComponent<PositionManager>().GetPositionAt(new GameTick(0))).Magnitude() - (centroid - b.GetComponent<PositionManager>().GetPositionAt(new GameTick(0))).Magnitude()));

                    // Determine the distance of the farthest outpost
                    float farthestDistance = (centroid - closestOutposts[_mapConfiguration.OutpostsPerPlayer - 1].GetComponent<PositionManager>().GetPositionAt(new GameTick(0))).Magnitude();

                    // If the current outpost is closer, put the current outpost in the list, replacing the farther outpost.
                    if (currentDistance < farthestDistance)
                    {
                        closestOutposts[_mapConfiguration.OutpostsPerPlayer - 1] = o;
                    }
                }
            }

            // Once the closest outposts are determined, set the owner.
            // And provide some drillers.
            foreach (Outpost closeOutpost in closestOutposts)
            {
                closeOutpost.GetComponent<DrillerCarrier>().SetOwner(player);
                closeOutpost.GetComponent<DrillerCarrier>().AddDrillers(Constants.InitialDrillersPerOutpost);
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
            if (this._mapConfiguration.OutpostsPerPlayer > 0 && _players.Count > 0)
                this.SetOutpostOwner(firstPlayerOutposts, _players[0]);

            // Tile the outposts based on the # of players on a 2xn grid
            // Example:
            //
            // 1 3 5 7 9
            // 2 4 6 8 10
            //
            // Each # is a player, each player has a (2 * maxSeedDistance)x(2 * maxSeedDistance) area
            int width = (int)(Math.Ceiling(_players.Count / 2.0f)); // calculate n
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
                    if (playersGenerated < _players.Count)
                    {
                        // Translate the first player's outposts based on the width/height counter for this player.
                        // Function also applies a random rotation.
                        List<Outpost> translatedOutposts = this.TranslateOutposts(firstPlayerOutposts, new RftVector(_map, _mapConfiguration.MaximumOutpostDistance * widthCounter * 2, _mapConfiguration.MaximumOutpostDistance * heightCounter * 2));

                        // Update the owner to the new player
                        bool queenGenerated = false;
                        foreach (Outpost o in translatedOutposts)
                        {
                            if (o.GetComponent<DrillerCarrier>().GetOwner() != null)
                                o.GetComponent<DrillerCarrier>().SetOwner(_players[(widthCounter - 1) * 2 + (heightCounter - 1)]);
                            if (!queenGenerated)
                            {
                                o.GetComponent<SpecialistManager>().AddSpecialist(new Queen(o.GetComponent<DrillerCarrier>().GetOwner()));
                                queenGenerated = true;
                            }
                        }

                        // Add the outposts to the generated list.
                        this._outposts.AddRange(translatedOutposts);

                        // Increment the map generation loop
                        heightCounter++;
                        playersGenerated++;
                    }
                    else
                    {
                        // Return the outposts if we stop early.
                        return this._outposts;
                    }
                }
                // After a height of 2, reset the height to 1 but increase the width
                widthCounter++;
                heightCounter = 1;
            }

            // Return a list of the generated outposts.
            return this._outposts;
        }

        /// <summary>
        /// Creates an outpost of the given type and given position.
        /// </summary>
        /// <param name="outpostPosition">Position of outpost</param>
        /// <param name="type"></param>
        /// <returns></returns>
        private Outpost CreateOutpost(RftVector outpostPosition, OutpostType type)
        {
            switch (type)
            {
                case OutpostType.Factory: return new Factory(_generator.GetNextId(), outpostPosition);
                case OutpostType.Generator: return new Generator(_generator.GetNextId(), outpostPosition);
                case OutpostType.Mine: return new Mine(_generator.GetNextId(), outpostPosition);
                case OutpostType.Watchtower: return new Watchtower(_generator.GetNextId(), outpostPosition);
                default: throw new OutpostTypeException("Invalid Outpost Type");
            }
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

    public class OutpostTypeException : Exception
    {
        public OutpostTypeException(string message) : base(message)
        {

        }
    }
}
  

