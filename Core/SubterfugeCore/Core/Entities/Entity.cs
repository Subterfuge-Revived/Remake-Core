using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Components;

namespace SubterfugeCore.Core.Entities
{
    public abstract class Entity
    {
        List<EntityComponent> components = new List<EntityComponent>();

        public void AddComponent(EntityComponent component)
        {
            this.components.Add(component);
        }

        public T GetComponent<T>()
            where T : EntityComponent
        {
            return components.OfType<T>().First();
        }

        public bool HasComponent<T>()
            where T : EntityComponent
        {
            return components.OfType<T>().Any();
        }
    }
}