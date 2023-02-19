using System;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities.Specialists;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ISpecialistCarrierEventPublisher
    {
        event EventHandler<OnAddSpecialistEventArgs> OnAddSpecialist;
        event EventHandler<OnRemoveSpecialistEventArgs> OnRemoveSpecialist;
        event EventHandler<OnSpecialistCapacityChangeEventArgs> OnSpecialistCapacityChange;
    }
    
    public class OnAddSpecialistEventArgs
    {
        public Specialist AddedSpecialist { get; set; }
        public SpecialistManager AddedTo { get; set; }
    }
    
    public class OnRemoveSpecialistEventArgs
    {
        public Specialist RemovedSpecialist { get; set; }
        public SpecialistManager RemovedFrom { get; set; }
    }
    
    public class OnSpecialistCapacityChangeEventArgs
    {
        public int PreviousCapacity { get; set; }
        public int NewCapacity { get; set; }
        public SpecialistManager SpecialistManager { get; set; }
    }
}