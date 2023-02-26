using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities
{
    /// <summary>
    /// An instance of a Sub
    /// </summary>
    public class Sub : Entity
    {
        /// <summary>
        /// Sub constructor
        /// </summary>
        /// <param name="id">The ID of the sub</param>
        /// <param name="source">The initial location of the sub</param>
        /// <param name="destination">The destination of the sub</param>
        /// <param name="launchTime">The time of launch</param>
        /// <param name="drillerCount">The amount of drillers to launch</param>
        /// <param name="owner">The owner</param>
        public Sub(
            string id,
            IEntity source,
            IEntity destination,
            GameTick launchTime,
            int drillerCount,
            Player owner,
            TimeMachine timeMachine
        )
        {
            AddComponent(new DrillerCarrier(this, drillerCount, owner));
            AddComponent(new SpeedManager(this, 1.0f));
            AddComponent(new PositionManager(this, source, destination, launchTime, timeMachine));
            AddComponent(new SpecialistManager(this));
            AddComponent(new IdentityManager(this, id));
            AddComponent(new ShieldManager(this, 0));
            AddComponent(new SubLauncher(this));
            AddComponent(new VisionManager(this, Constants.BaseOutpostVisionRadius * 0.2f));
        }
    }
}
