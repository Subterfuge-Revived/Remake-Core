using System.Collections.Generic;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.Combat.CombatEvents
{
    public class EntityExplodeEffect : NaturalGameEvent
    {
        
        private IEntity _entityExploding;
        private GameState _state;

        private List<IEntity> entitiesDestroyed;
        
        public EntityExplodeEffect(
            GameTick occursAt,
            IEntity entityExploding,
            GameState currentState
        ) : base(occursAt, Priority.SPECIALIST_EXPLODE)
        {
            _entityExploding = entityExploding;
            _state = currentState;
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            entitiesDestroyed = _state.EntitesInRange(Constants.BaseSubVisionRadius,
                _entityExploding.GetComponent<PositionManager>().CurrentLocation);
            
            // Remove all of the entities from the game state.
            foreach(IEntity e in entitiesDestroyed)
            {
                if (e is Sub)
                {
                    state.RemoveSub(e as Sub);
                }
                else
                {
                    e.GetComponent<DrillerCarrier>().Destroy();
                }
            }

            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            foreach(IEntity e in entitiesDestroyed)
            {
                if (e is Sub)
                {
                    state.AddSub(e as Sub);
                }
                else
                {
                    // TODO: This is not perfect... Need to ensure that we can "redo" a destroy event somehow.
                    e.GetComponent<DrillerCarrier>().UndoDestroy(10);
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