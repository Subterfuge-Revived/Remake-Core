using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ICombatEventPublisher
    {
        event EventHandler<OnPreCombatEventArgs> OnPreCombat;
        event EventHandler<PostCombatEventArgs> OnPostCombat;
    }

    public class OnPreCombatEventArgs
    {
        public IEntity ParticipantOne { get; set; }
        public IEntity ParticipantTwo { get; set; }
        public TimeMachineDirection Direction { get; set; }
    }

    public class PostCombatEventArgs
    {
        public IEntity Winner { get; set; }
        public TimeMachineDirection Direction { get; set; }
        public CombatCleanup CombatSummary { get; set; }
    }
}