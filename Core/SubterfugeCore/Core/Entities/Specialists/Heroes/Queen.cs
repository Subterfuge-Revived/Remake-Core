/*using System;
using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Heroes
{
    public class Queen: Specialist
    {
        public Queen(Player owner) : base(owner, true)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!this._isCaptured)
            {
                entity.GetComponent<ShieldManager>().AddShield(25);
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!this._isCaptured)
            {
                entity.GetComponent<ShieldManager>().RemoveShields(25);
            }
        }

        public override void OnCapturedEvent(IEntity captureLocation)
        {
            // Check for princesses to take over.
            // Otherwise, kill player.
            captureLocation.GetComponent<DrillerCarrier>().GetOwner().SetEliminated(true);
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Queen;
        }

        public override void SpecialistTransferEvent(object? sender, OnSpecialistTransferEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override void OnCapturedEvent(object? sender, OnSpecialistsCapturedEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public override List<NaturalGameEvent> CreateEffects(object? sender, EventArgs subscribedEvent)
        {
            throw new NotImplementedException();
        }
    }
}*/