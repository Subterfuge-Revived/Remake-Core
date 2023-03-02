using System;
using Subterfuge.Remake.Core.Entities;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface ICourseChangeEventPublisher
    {
        event EventHandler<OnCourseChangeEventArgs> OnCourseChange;
    }

    public class OnCourseChangeEventArgs: DirectionalEventArgs
    {
        public Sub Sub { get; set; }
        public IEntity OriginalDestination { get; set; }
        public IEntity NewDestination { get; set; }
    }
    
}