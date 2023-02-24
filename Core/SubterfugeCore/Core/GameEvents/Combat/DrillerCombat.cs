using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Validators;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat
{
    /// <summary>
    /// Driller combat action
    /// </summary>
    public class DrillerCombat : IReversible
    {
        private readonly IEntity _combatant1;
        private readonly IEntity _combatant2;
        private bool _eventSuccess;

        private int _preCombatDrillers1;
        private int _preCombatDrillers2;

        /// <summary>
        /// Driller combat constructor
        /// </summary>
        /// <param name="combatant1">Combatant 1</param>
        /// <param name="combatant2">Combatant 2</param>
        public DrillerCombat(IEntity combatant1, IEntity combatant2)
        {
            this._combatant1 = combatant1;
            this._combatant2 = combatant2;
        }

        /// <summary>
        /// Performs the reverse action of the driller combat to undo.
        /// </summary>
        /// <returns>if the event was reversed</returns>
        public bool BackwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            if (_eventSuccess)
            {
                // Restore driller counts.
                _combatant1.GetComponent<DrillerCarrier>().SetDrillerCount(_preCombatDrillers1);
                _combatant2.GetComponent<DrillerCarrier>().SetDrillerCount(_preCombatDrillers2);
            }
            return _eventSuccess;
        }

        /// <summary>
        /// Performs driller combat between two subs
        /// </summary>
        /// <returns>If the event was succesfull</returns>
        public bool ForwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            if (Validator.ValidateICombatable(state, _combatant1) && Validator.ValidateICombatable(state, _combatant2))
            {
                _preCombatDrillers1 = _combatant1.GetComponent<DrillerCarrier>().GetDrillerCount();
                _preCombatDrillers2 = _combatant2.GetComponent<DrillerCarrier>().GetDrillerCount();
                _combatant1.GetComponent<DrillerCarrier>().AlterDrillers(_preCombatDrillers2 * -1);
                _combatant2.GetComponent<DrillerCarrier>().AlterDrillers(_preCombatDrillers1 * -1);
                this._eventSuccess = true; 
            }
            else
            {
                this._eventSuccess = false;
            }

            return this._eventSuccess;
        }
        
        /// <summary>
        /// Determines if the event was successful.
        /// </summary>
        /// <returns>If the event is successful</returns>
        public bool WasEventSuccessful()
        {
            return this._eventSuccess;
        }
    }
}
