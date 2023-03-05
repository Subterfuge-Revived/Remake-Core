using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class NoOpSpecialist : Specialist
    {
        public NoOpSpecialist(
            Player owner
        ) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Advisor;
        }

        public override string GetDescription()
        {
            return $"Does nothing";
        }
    }
}