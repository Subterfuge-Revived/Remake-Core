using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Entities;

namespace SubterfugeCore.Core.Components
{

    public abstract class EntityComponent
    {
        public Entity Parent { get; set; }
        
        public EntityComponent(Entity parent)
        {
            Parent = parent;
        }
    }
}