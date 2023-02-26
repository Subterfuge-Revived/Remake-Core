using System;
using System.Collections.Generic;
using Subterfuge.Remake.Core.Entities.Components;

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
                return _componentMap[typeof(T)] as T ?? throw new ComponentDoesNotExistException($"Found component {typeof(T)} but it was null.");
            }
            throw new ComponentDoesNotExistException($"Looking for component of {typeof(T)} but the component was not present.");
        }

        public bool HasComponent<T>()
            where T : EntityComponent
        {
            return _componentMap.ContainsKey(typeof(T));

        }
    }

    public interface IEntity
    {
        bool HasComponent<T>() where T : EntityComponent;
        T GetComponent<T>() where T : EntityComponent;
        void AddComponent(EntityComponent component);
    }

    public class ComponentDoesNotExistException : Exception
    {
        public ComponentDoesNotExistException(string detail) : base(detail){}
    }
}