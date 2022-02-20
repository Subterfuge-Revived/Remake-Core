using System;
using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Components;

namespace SubterfugeCore.Core.Entities
{
    public abstract class Entity : IEntity
    {
        Dictionary<Type, EntityComponent> componentMap = new Dictionary<Type, EntityComponent>();

        public void AddComponent(EntityComponent component)
        {
            componentMap[component.GetType()] = component;
        }

        public T GetComponent<T>()
            where T : EntityComponent
        {
            if (componentMap.ContainsKey(typeof(T)))
            {
                return componentMap[typeof(T)] as T;
            }
            return null;
        }

        public bool HasComponent<T>()
            where T : EntityComponent
        {
            if (componentMap.ContainsKey(typeof(T)))
            {
                return componentMap[typeof(T)] != null;
            }
            return false;
        }
    }

    public interface IEntity
    {
        bool HasComponent<T>() where T : EntityComponent;
        T GetComponent<T>() where T : EntityComponent;
        void AddComponent(EntityComponent component);

    }
}