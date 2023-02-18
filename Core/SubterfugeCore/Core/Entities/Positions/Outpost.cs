using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Config;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Core.Entities.Positions
{
	/// <summary>
	/// Outpost class
	/// </summary>
    public abstract class Outpost : Entity
    {
        /// <summary>
        /// Outpost constructor
        /// </summary>
        /// <param name="id">The outpost id</param>
        /// <param name="outpostStartPosition">The position of the outpost</param>
        /// <param name="outpostOwner">The owner of the outpost</param>
        /// <param name="visionRadius">The vision radius</param>
        public Outpost(
            string id,
            RftVector outpostStartPosition,
            Player outpostOwner = null,
            float visionRadius = Constants.BaseOutpostVisionRadius
            )
        {
            AddComponent(new DrillerCarrier(this, Constants.InitialDrillersPerOutpost, outpostOwner));
            AddComponent(new SpeedManager(this, 0.0f));
            AddComponent(new PositionManager(this, outpostStartPosition, null, new GameTick()));
            AddComponent(new SpecialistManager(this, 100));
            AddComponent(new IdentityManager(this, id));
            AddComponent(new ShieldManager(this, Constants.InitialMaxShieldsPerOutpost));
            AddComponent(new SubLauncher(this));
            AddComponent(new VisionManager(this, visionRadius));
        }

        /// <summary>
        /// Outpost constructor; should only be used when it is necessary to change the type of an outpost (i.e. when converting an outpost to a mine)
        /// </summary>
        /// <param name="o">The outpost to copy components from</param>
        public Outpost(Outpost o)
        {
            AddComponent(o.GetComponent<DrillerCarrier>());
            AddComponent(o.GetComponent<SpeedManager>());
            AddComponent(o.GetComponent<PositionManager>());
            AddComponent(o.GetComponent<SpecialistManager>());
            AddComponent(o.GetComponent<IdentityManager>());
            AddComponent(o.GetComponent<ShieldManager>());
            AddComponent(o.GetComponent<SubLauncher>());
            AddComponent(o.GetComponent<VisionManager>());
        }

        /// <summary>
        /// Gets the outpost type
        /// </summary>
        /// <returns>The type of outpost</returns>
        public abstract OutpostType GetOutpostType();
    }
}
