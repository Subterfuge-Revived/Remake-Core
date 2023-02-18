using Subterfuge.Remake.Core.Entities;

namespace Subterfuge.Remake.Core.Components
{

    public abstract class EntityComponent
    {
        public IEntity Parent { get; set; }

        protected EntityComponent(IEntity parent)
        {
            Parent = parent;
        }
    }
}