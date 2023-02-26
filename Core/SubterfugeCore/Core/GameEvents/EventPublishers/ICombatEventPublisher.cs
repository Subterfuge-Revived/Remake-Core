using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.Combat;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents.CombatResolve;
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
        public GameState CurrentState { get; set; }
        public CombatEvent CombatEvent { get; set; }
    }

    public class PostCombatEventArgs
    {
        public TimeMachineDirection Direction { get; set; }
        public GameState CurrentState { get; set; }
        public CombatResolution CombatResolution { get; set; }
    }
}