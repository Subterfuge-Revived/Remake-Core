using System;
using System.Collections.Generic;
using Subterfuge.Remake.Core.Components;

namespace Subterfuge.Remake.Core.Entities
{
    public abstract class Entity : IEntity
    {
        private readonly Dictionary<Type, EntityComponent> _componentMap = new Dictionary<Type, EntityComponent>();

        public void AddComponent(EntityComponent component)
        {
            _componentMap[component.GetType()] = component;
        }

        public T GetComponent<T>()
            where T : EntityComponent
        {
            if (_componentMap.ContainsKey(typeof(T)))
            {
                return _componentMap[typeof(T)] as T;
            }
            return null;
        }

        public bool HasComponent<T>()
            where T : EntityComponent
        {
            if (_componentMap.ContainsKey(typeof(T)))
            {
                return _componentMap[typeof(T)] != null;
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