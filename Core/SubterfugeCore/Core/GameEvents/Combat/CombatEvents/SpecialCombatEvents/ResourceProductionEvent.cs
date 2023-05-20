using System;
using Microsoft.DotNet.PlatformAbstractions;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class ResourceProductionEvent : PositionalGameEvent
    {
        public readonly IEntity ProductionLocation;
        public bool _ignoresCapacity = false;
        public ProducedResourceType ResourceType { get; }
        public int ValueProduced;
        
        public ResourceProductionEvent(
            GameTick occursAt,
            IEntity locationProducedAt,
            ProducedResourceType producedType,
            int valueProduced,
            bool ignoresCapacity
        ) : base(occursAt, Priority.RESOURCE_PRODUCTION, locationProducedAt)
        {
            ProductionLocation = locationProducedAt;
            ResourceType = producedType;
            ValueProduced = valueProduced;
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            switch (ResourceType)
            {
                case ProducedResourceType.Driller:
                    // Ensure unowned outposts do not produce drillers
                    if (ProductionLocation.GetComponent<DrillerCarrier>().GetOwner() == null)
                    {
                        ValueProduced = 0;
                    }
                    else
                    {
                        ValueProduced = ProductionLocation.GetComponent<DrillerCarrier>().AlterDrillers(GetNextProductionAmount(timeMachine.GetState()));
                    }
                    break;
                case ProducedResourceType.Neptunium:
                    ValueProduced = ProductionLocation.GetComponent<DrillerCarrier>().GetOwner().AlterNeptunium(GetNextProductionAmount(timeMachine.GetState()));
                    break;
                case ProducedResourceType.Shield:
                    // Ensure unowned outposts do not produce shields
                    if (ProductionLocation.GetComponent<DrillerCarrier>().GetOwner() == null)
                    {
                        ValueProduced = 0;
                    }
                    else
                    {
                        ValueProduced = ProductionLocation.GetComponent<ShieldManager>().AlterShields(GetNextProductionAmount(timeMachine.GetState()));
                    }
                    break;
            }

            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            switch (ResourceType)
            {
                case ProducedResourceType.Driller:
                    ProductionLocation.GetComponent<DrillerCarrier>().AlterDrillers(-1 * ValueProduced);
                    break;
                case ProducedResourceType.Neptunium:
                    ProductionLocation.GetComponent<DrillerCarrier>().GetOwner().AlterNeptunium(-1 * ValueProduced);
                    break;
                case ProducedResourceType.Shield:
                    ProductionLocation.GetComponent<ShieldManager>().AlterShields(-1 * ValueProduced);
                    break;
            }
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }

        public int GetNextProductionAmount(GameState state)
        {
            var owner = ProductionLocation.GetComponent<DrillerCarrier>().GetOwner();
            if (ProductionLocation.GetComponent<DrillerCarrier>().IsDestroyed() || (owner != null && owner.IsEliminated()))
            {
                return 0;
            }
            
            switch (ResourceType)
            {
                case ProducedResourceType.Driller:
                    var extraDrillerCapacity = state.GetExtraDrillerCapacity(owner);

                    if (!_ignoresCapacity)
                    {
                        // Get the min of the extra capacity vs. production count.
                        return Math.Min(Math.Max(0, extraDrillerCapacity), ValueProduced);
                    }
                    
                    return ValueProduced;
                case ProducedResourceType.Neptunium:
                    return state.GetPlayerOutposts(ProductionLocation.GetComponent<DrillerCarrier>().GetOwner()).Count;
                case ProducedResourceType.Shield:
                    return ValueProduced;
                default:
                    return ValueProduced;
            }
        }
        
        public override bool Equals(object other)
        {
            ResourceProductionEvent asEvent = other as ResourceProductionEvent;
            if (asEvent == null)
                return false;

            return asEvent.OccursAt == this.OccursAt &&
                   asEvent.Priority == this.Priority &&
                   asEvent.ProductionLocation == this.ProductionLocation &&
                   asEvent.ResourceType == this.ResourceType;
        }

        public override int GetHashCode()
        {
            var hashBuilder = new HashCodeCombiner();
            hashBuilder.Add(OccursAt);
            hashBuilder.Add(Priority);
            hashBuilder.Add(ProductionLocation);
            hashBuilder.Add(ResourceType);
            return hashBuilder.GetHashCode();
        }
    }

    public enum ProducedResourceType
    {
        Unknown = 0,
        Driller = 1,
        Shield = 2,
        Neptunium = 3,
    }
}