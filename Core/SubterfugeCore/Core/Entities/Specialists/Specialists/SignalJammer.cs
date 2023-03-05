using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class SignalJammer : Specialist
    {
        private List<float> SpeedReductionPerLevel = new List<float>() { 0.10f, 0.20f, 0.30f };
        
        public SignalJammer(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnLocationTargeted += OnTargetedBySub;
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnLocationTargeted -= OnTargetedBySub;
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<PositionManager>().OnLocationTargeted -= OnTargetedBySub;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.SignalJammer;
        }

        public override string GetDescription()
        {
            return $"When the Signal Jammer's location is targeted by an enemy, the enemy is slowed by {SpeedReductionPerLevel}.";
        }

        public void OnTargetedBySub(object? sender, OnLocationTargetedEventArgs eventArgs)
        {
            eventArgs.TargetedBy.GetComponent<SpeedManager>().DecreaseSpeed(0.15f * _level);
        }
    }
}