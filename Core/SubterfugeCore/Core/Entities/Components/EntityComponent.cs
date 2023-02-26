namespace Subterfuge.Remake.Core.Entities.Components
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