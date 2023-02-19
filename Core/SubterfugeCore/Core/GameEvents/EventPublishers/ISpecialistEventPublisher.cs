using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ISpecialistEventPublisher
    {
        event EventHandler<OnSpecialistHireEventArgs> OnSpecialistHire;
        event EventHandler<OnSpecialistCaptureEventArgs> OnSpecialistCapture;
        event EventHandler<OnSpecialistUncaptureEventArgs> OnSpecialistUncapture;
        event EventHandler<OnSpecialistPromotionEventArgs> OnSpecialistPromotion;
    }

    public class OnSpecialistHireEventArgs
    {
        public Specialist HiredSpecialist { get; set; }
        public IEntity HireLocation { get; set; }
    }

    public class OnSpecialistCaptureEventArgs
    {
        public Specialist Specialist { get; set; }
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
}