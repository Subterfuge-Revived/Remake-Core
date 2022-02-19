using Google.Protobuf;
using SubterfugeCore.Core.Entities.Specialists;

namespace SubterfugeCore.Core.EventArgs
{
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
        public int previousCapacity { get; set; }
        public int newCapacity { get; set; }
        public SpecialistManager SpecialistManager { get; set; }
    }

    public class OnSpecialistCapturedEventArgs
    {
        public Specialist CapturedSpecialist { get; set; }
        public SpecialistManager SpecialistManager { get; set; }
    }

    public class OnSpecialistUncapturedEventArgs
    {
        public Specialist UncapturedSpecialist { get; set; }
        public SpecialistManager SpecialistManager { get; set; }
    }
}