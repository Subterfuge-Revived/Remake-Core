using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Entities;

namespace SubterfugeCore.Core.Components
{

    public abstract class EntityComponent
    {
        public IEntity Parent { get; set; }
        
        public EntityComponent(IEntity parent)
        {
            Parent = parent;
        }
    }
}