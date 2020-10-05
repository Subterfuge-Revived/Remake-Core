using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents.Validators;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.GameEvents.ReversibleEvents
{
    /// <summary>
    /// Driller combat action
    /// </summary>
    public class DrillerCombat : IReversible
    {
        ICombatable _combatant1;
        ICombatable _combatant2;
        bool _eventSuccess = false;

        int _preCombatDrillers1;
        int _preCombatDrillers2;

        /// <summary>
        /// Driller combat constructor
        /// </summary>
        /// <param name="combatant1">Combatant 1</param>
        /// <param name="combatant2">Combatant 2</param>
        public DrillerCombat(ICombatable combatant1, ICombatable combatant2)
        {
            this._combatant1 = combatant1;
            this._combatant2 = combatant2;
        }

        /// <summary>
        /// Performs the reverse action of the driller combat to undo.
        /// </summary>
        /// <returns>if the event was reversed</returns>
        public bool BackwardAction()
        {
            if (_eventSuccess)
            {
                // Restore driller counts.
                _combatant1.SetDrillerCount(_preCombatDrillers1);
                _combatant2.SetDrillerCount(_preCombatDrillers2);
            }
            return _eventSuccess;
        }

        /// <summary>
        /// Performs driller combat between two subs
        /// </summary>
        /// <returns>If the event was succesfull</returns>
        public bool ForwardAction()
        {
            if (Validator.ValidateICombatable(_combatant1) && Validator.ValidateICombatable(_combatant2))
            {
                _preCombatDrillers1 = _combatant1.GetDrillerCount();
                _preCombatDrillers2 = _combatant2.GetDrillerCount();
                _combatant1.RemoveDrillers(_preCombatDrillers2);
                _combatant2.RemoveDrillers(_preCombatDrillers1);
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
