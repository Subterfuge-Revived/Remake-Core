using SubterfugeCore.Core.Entities;

namespace SubterfugeCore.Core.Components
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