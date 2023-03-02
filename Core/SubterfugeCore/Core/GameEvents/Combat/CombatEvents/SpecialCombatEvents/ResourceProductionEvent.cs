using Microsoft.DotNet.PlatformAbstractions;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class ResourceProductionEvent : PositionalGameEvent
    {
        private readonly IEntity _productionLocation;
        public ProducedResourceType ResourceType { get; }
        private int _valueToProduce;
        
        public ResourceProductionEvent(
            GameTick occursAt,
            IEntity locationProducedAt,
            ProducedResourceType producedType,
            int valueToProduce
        ) : base(occursAt, Priority.RESOURCE_PRODUCTION, locationProducedAt)
        {
            _productionLocation = locationProducedAt;
            ResourceType = producedType;
            _valueToProduce = valueToProduce;
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            switch (ResourceType)
            {
                case ProducedResourceType.Driller:
                    _valueToProduce = _productionLocation.GetComponent<DrillerCarrier>().AlterDrillers(GetNextProductionAmount(state));
                    break;
                case ProducedResourceType.Neptunium:
                    _valueToProduce = _productionLocation.GetComponent<DrillerCarrier>().GetOwner().AlterNeptunium(GetNextProductionAmount(state));
                    break;
                case ProducedResourceType.Shield:
                    _valueToProduce = _productionLocation.GetComponent<ShieldManager>().AlterShields(GetNextProductionAmount(state));
                    break;
            }

            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            switch (ResourceType)
            {
                case ProducedResourceType.Driller:
                    _productionLocation.GetComponent<DrillerCarrier>().AlterDrillers(-1 * _valueToProduce);
                    break;
                case ProducedResourceType.Neptunium:
                    _productionLocation.GetComponent<DrillerCarrier>().GetOwner().AlterNeptunium(-1 * _valueToProduce);
                    break;
                case ProducedResourceType.Shield:
                    _productionLocation.GetComponent<ShieldManager>().AlterShields(-1 * _valueToProduce);
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
            var owner = _productionLocation.GetComponent<DrillerCarrier>().GetOwner();
            if (_productionLocation.GetComponent<DrillerCarrier>().IsDestroyed() || (owner != null && owner.IsEliminated()))
            {
                return 0;
            }
            
            switch (ResourceType)
            {
                case ProducedResourceType.Driller:
                    return _valueToProduce;
                case ProducedResourceType.Neptunium:
                    return state.GetPlayerOutposts(_productionLocation.GetComponent<DrillerCarrier>().GetOwner()).Count;
                case ProducedResourceType.Shield:
                    return _valueToProduce;
                default:
                    return _valueToProduce;
            }
        }
        
        public override bool Equals(object other)
        {
            ResourceProductionEvent asEvent = other as ResourceProductionEvent;
            if (asEvent == null)
                return false;

            return asEvent.OccursAt == this.OccursAt &&
                   asEvent.Priority == this.Priority &&
                   asEvent._productionLocation == this._productionLocation &&
                   asEvent.ResourceType == this.ResourceType;
        }

        public override int GetHashCode()
        {
            var hashBuilder = new HashCodeCombiner();
            hashBuilder.Add(OccursAt);
            hashBuilder.Add(Priority);
            hashBuilder.Add(_productionLocation);
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