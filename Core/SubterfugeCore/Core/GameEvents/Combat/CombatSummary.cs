using System.Collections.Generic;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat
{
    public class CombatSummary
    {
        public int TotalDrillersDestroyed { get; set; }
        public IEntity VictoriousEntity { get; set; }
        public List<Specialist> KilledSpecialists { get; set; }
        public bool WasSubDestroyed { get; set; } = false;
        public bool WasOutpostCaptured { get; set; } = false;
        public bool WasFriendlyCombat { get; set; }
    }
}