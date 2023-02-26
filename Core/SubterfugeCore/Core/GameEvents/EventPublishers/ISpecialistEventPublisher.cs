﻿using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ISpecialistEventPublisher
    {
        event EventHandler<OnSpecialistHireEventArgs> OnSpecialistHire;
        event EventHandler<OnSpecialistsCapturedEventArgs> OnCaptured;
        event EventHandler<OnSpecialistPromotionEventArgs> OnSpecialistPromotion;
        event EventHandler<OnAddSpecialistEventArgs> OnSpecialistArrive;
        event EventHandler<OnRemoveSpecialistEventArgs> OnSpecialistLeave;
        event EventHandler<OnSpecialistCapacityChangeEventArgs> OnSpecialistCapacityChange;
    }

    public class OnSpecialistHireEventArgs
    {
        public Specialist HiredSpecialist { get; set; }
        public IEntity HireLocation { get; set; }
    }

    public class OnSpecialistsCapturedEventArgs
    {
        public IEntity CaptureLocation { get; set; }
    }
    
    public class OnSpecialistUncaptureEventArgs
    {
        public Specialist Specialist { get; set; }
        public IEntity CapturedFrom { get; set; }
        public IEntity Destination { get; set; }
    }

    public class OnSpecialistPromotionEventArgs
    {
        public Specialist PromotedTo { get; set; }
        public IEntity Location { get; set; }
    }
    
    public class OnAddSpecialistEventArgs
    {
        public Specialist AddedSpecialist { get; set; }
        public IEntity AddedTo { get; set; }
    }

    public class OnRemoveSpecialistEventArgs
    {
        public Specialist RemovedSpecialist { get; set; }
        public IEntity RemovedFrom { get; set; }
    }

    public class OnSpecialistCapacityChangeEventArgs
    {
        public int CapacityDelta { get; set; }
    }
}