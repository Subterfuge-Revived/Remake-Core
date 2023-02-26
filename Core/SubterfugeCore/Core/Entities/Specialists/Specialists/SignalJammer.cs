using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class SignalJammer : Specialist
    {
        public SignalJammer(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnLocationTargeted += OnTargetedBySub;
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<PositionManager>().OnLocationTargeted -= OnTargetedBySub;
            }
        }

        public override void OnCaptured(IEntity captureLocation)
        {
            captureLocation.GetComponent<PositionManager>().OnLocationTargeted -= OnTargetedBySub;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.SignalJammer;
        }

        public void OnTargetedBySub(object? sender, OnLocationTargetedEventArgs eventArgs)
        {
            eventArgs.TargetedBy.GetComponent<SpeedManager>().DecreaseSpeed(0.15f * _level);
        }
    }
}