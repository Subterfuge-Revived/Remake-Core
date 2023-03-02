using System;
using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Combat;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.GameEvents.SpecialistEvents;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Advisor : Specialist
    {
        public Advisor(Player owner) : base(owner, false)
        {
        }
        

        protected override List<GameEvent> ForwardEffects(object? sender, DirectionalEventArgs subscribedEvent)
        {
            OnSpecialistTransferEventArgs transferEvent = subscribedEvent as OnSpecialistTransferEventArgs; 
            return new List<GameEvent>()
            {
                new SpecialistCapacityChangeEvent(
                    subscribedEvent.TimeMachine.GetCurrentTick(),
                    transferEvent.AddedTo,
                    1
                )
            };
        }

        protected override List<GameEvent> CaptureEffects(object? sender, OnSpecialistsCapturedEventArgs subscribedEvent)
        { 
            return new List<GameEvent>()
            {
                new SpecialistCapacityChangeEvent(
                    subscribedEvent.TimeMachine.GetCurrentTick(),
                    subscribedEvent.Location,
                    -1
                )
            };
        }

        public override void ArriveAtLocation(IEntity location)
        {
            location.GetComponent<SpecialistManager>().OnSpecialistTransfer += TriggerForwardEffect;
        }

        public override void LeaveLocation(IEntity location)
        {
            location.GetComponent<SpecialistManager>().OnSpecialistTransfer -= TriggerForwardEffect;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Advisor;
        }
    }
}