using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents.CombatResolve
{
    public abstract class CombatResolution : PositionalGameEvent
    {
        public IEntity Winner { get; set; }
        public IEntity Loser { get; set; }
        public bool IsTie { get; set; } = false;
        protected CombatType _combatType; 
        
        protected CombatResolution(
            GameTick occursAt,
            CombatType combatType,
            IEntity expectedWinLocation
        ) : base(occursAt, Priority.COMBAT_RESOLVE, expectedWinLocation)
        {
            _combatType = combatType;
        }
    }
}