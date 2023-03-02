using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class NoOpSpecialist : Specialist
    {
        public NoOpSpecialist(
            Player owner
        ) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity location)
        {
            return;
        }

        public override void LeaveLocation(IEntity location)
        {
            return;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Advisor;
        }

        protected override List<GameEvent> ForwardEffects(object? sender, DirectionalEventArgs subscribedEvent)
        {
            return new List<GameEvent>();
        }

        protected override List<GameEvent> CaptureEffects(object? sender, OnSpecialistsCapturedEventArgs capturedEvent)
        {
            return new List<GameEvent>();
        }
    }
}