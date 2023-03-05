using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.Combat;
using Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents.CombatResolve;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ICombatEventPublisher
    {
        event EventHandler<OnRegisterCombatEventArgs> OnRegisterCombatEffects;
        event EventHandler<OnLocationTargetedEventArgs> OnLocationTargeted;
    }

    public class OnRegisterCombatEventArgs: DirectionalEventArgs
    {
        public GameState CurrentState { get; set; }
        public CombatEvent CombatEvent { get; set; }
    }

    public class OnLocationTargetedEventArgs: DirectionalEventArgs
    {
        public IEntity TargetedBy { get; set; }
        public IEntity Destination { get; set; }
    }
}