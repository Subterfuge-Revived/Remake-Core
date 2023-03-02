using System;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ISpecialistEventPublisher
    {
        event EventHandler<OnSpecialistHireEventArgs> OnSpecialistHire;
        event EventHandler<OnSpecialistsCapturedEventArgs> OnSpecialistCapture;
        event EventHandler<OnSpecialistPromotionEventArgs> OnSpecialistPromotion;
        event EventHandler<OnSpecialistTransferEventArgs> OnSpecialistTransfer;
    }

    public class OnSpecialistHireEventArgs : DirectionalEventArgs
    {
        public Specialist HiredSpecialist { get; set; }
        public IEntity HireLocation { get; set; }
    }

    public class OnSpecialistsCapturedEventArgs: DirectionalEventArgs
    {
        public IEntity Location { get; set; }
    }

    public class OnSpecialistPromotionEventArgs: DirectionalEventArgs
    {
        public Specialist PromotedTo { get; set; }
        public IEntity Location { get; set; }
    }
    
    public class OnSpecialistTransferEventArgs: DirectionalEventArgs
    {
        public Specialist specialist { get; set; }
        public IEntity AddedTo { get; set; }
        public IEntity? RemovedFrom { get; set; }
    }
}