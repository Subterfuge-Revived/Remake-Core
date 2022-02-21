using System;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Entities.Positions
{
    public class OutpostFactory
    {
        public static Outpost CreateOutpost(
            OutpostType outpostType,
            Player owner,
            RftVector outpostLocation,
            int initialDrillers = Constants.InitialDrillersPerOutpost
        )
        {
            Outpost outpost = null;
            switch (outpostType)
            {
                case OutpostType.Factory:
                    outpost = new Factory(Guid.NewGuid().ToString(), outpostLocation, owner);
                    break;
                case OutpostType.Generator:
                    outpost = new Generator(Guid.NewGuid().ToString(), outpostLocation, owner);
                    break;
                case OutpostType.Mine:
                    outpost = new Mine(Guid.NewGuid().ToString(), outpostLocation, owner);
                    break;
                case OutpostType.Watchtower:
                    outpost = new Watchtower(Guid.NewGuid().ToString(), outpostLocation, owner);
                    break;
                default:
                    return null;
            }
            outpost.GetComponent<DrillerCarrier>().SetDrillerCount(initialDrillers);
            return outpost;
        }
        
        public static Outpost CreateOutpostWithId(
            string id,
            OutpostType outpostType,
            Player owner,
            RftVector outpostLocation,
            int initialDrillers = Constants.InitialDrillersPerOutpost
        )
        {
            Outpost outpost = null;
            switch (outpostType)
            {
                case OutpostType.Factory:
                    outpost = new Factory(id, outpostLocation, owner);
                    break;
                case OutpostType.Generator:
                    outpost = new Generator(id, outpostLocation, owner);
                    break;
                case OutpostType.Mine:
                    outpost = new Mine(id, outpostLocation, owner);
                    break;
                case OutpostType.Watchtower:
                    outpost = new Watchtower(id, outpostLocation, owner);
                    break;
                default:
                    return null;
            }
            outpost.GetComponent<DrillerCarrier>().SetDrillerCount(initialDrillers);
            return outpost;
        }
    }
}