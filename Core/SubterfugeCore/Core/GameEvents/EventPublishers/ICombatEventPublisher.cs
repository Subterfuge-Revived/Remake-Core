using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ICombatEventPublisher
    {
        event EventHandler<OnPreCombatEventArgs> OnPreCombat;
        event EventHandler<PostCombatEventArgs> OnPostCombat;
    }

    public class OnPreCombatEventArgs
    {
        public TimeMachineDirection Direction { get; set; }
        public CombatEvent CombatEvent { get; set; }
    }

    public class PostCombatEventArgs
    {
        public bool WasOutpostCaptured { get; set; }
        public Player WinningPlayer { get; set; }
        public IEntity SurvivingEntity { get; set; }
        public CombatCleanup CombatSummary { get; set; }
        public TimeMachineDirection Direction { get; set; }
    }
}