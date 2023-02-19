using System;
using Subterfuge.Remake.Core.Entities;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ICombatEventPublisher
    {
        event EventHandler<OnPreCombatEventArgs> OnPreCombat;
        event EventHandler<OnCombatVictoryEventArgs> OnCombatVictoryEventArgs;
        event EventHandler<OnCombatLossEventArgs> OnCombatLossEventArgs;
        event EventHandler<PostCombatEventArgs> OnPostCombat;
    }

    public class OnPreCombatEventArgs
    {
        public IEntity ParticipantOne { get; set; }
        public IEntity ParticipantTwo { get; set; }
        public TimeMachineDirection Direction { get; set; }
    }

    public class OnCombatVictoryEventArgs
    {
        public IEntity Winner { get; set; }
        public IEntity Loser { get; set; }
        public TimeMachineDirection Direction { get; set; }
    }

    public class OnCombatLossEventArgs
    {
        public IEntity Winner { get; set; }
        public IEntity Loser { get; set; }
        public TimeMachineDirection Direction { get; set; }
    }
    
    public class PostCombatEventArgs
    {
        public IEntity Winner { get; set; }
        public TimeMachineDirection Direction { get; set; }
    }
}