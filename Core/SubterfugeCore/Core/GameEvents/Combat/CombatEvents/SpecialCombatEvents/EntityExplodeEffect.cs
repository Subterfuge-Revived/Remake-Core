using System.Collections.Generic;
using System.Linq;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class EntityExplodeEffect : PositionalGameEvent
    {
        
        private IEntity _entityExploding;
        private GameState _state;

        private Dictionary<IEntity, EntityData> EntitiesBeforeExplosion;

        public EntityExplodeEffect(
            GameTick occursAt,
            IEntity entityExploding,
            GameState currentState
        ) : base(occursAt, Priority.SPECIALIST_EXPLODE, entityExploding)
        {
            _entityExploding = entityExploding;
            _state = currentState;
        }

        public override bool ForwardAction(TimeMachine timeMachine)
        {
            EntitiesBeforeExplosion = _state.EntitesInRange(Constants.BaseSubVisionRadius,
                _entityExploding.GetComponent<PositionManager>().CurrentLocation)
                .ToDictionary(entity => entity, entity => new EntityData()
                {
                    DrillerCount = entity.GetComponent<DrillerCarrier>().GetDrillerCount(),
                    ShieldCount = entity.GetComponent<ShieldManager>().GetShields(),
                    Specialists = entity.GetComponent<SpecialistManager>().GetSpecialists().ConvertAll(spec => spec.GetSpecialistId())
                });
            
            // Remove all of the entities from the game state.
            foreach(KeyValuePair<IEntity, EntityData> e in EntitiesBeforeExplosion)
            {
                if (e.Key is Sub)
                {
                    timeMachine.GetState().RemoveSub(e.Key as Sub);
                }
                else
                {
                    e.Key.GetComponent<DrillerCarrier>().SetDrillerCount(0);
                    e.Key.GetComponent<DrillerCarrier>().Destroy();
                }
            }

            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            foreach(KeyValuePair<IEntity, EntityData> e in EntitiesBeforeExplosion)
            {
                if (e.Key is Sub)
                {
                    timeMachine.GetState().AddSub(e.Key as Sub);
                }
                else
                {
                    e.Key.GetComponent<DrillerCarrier>().UndoDestroy(e.Value.DrillerCount);
                    e.Key.GetComponent<ShieldManager>().SetShields(e.Value.ShieldCount);
                }
            }

            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}