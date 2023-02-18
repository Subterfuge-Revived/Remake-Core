using System.Collections.Generic;
using Subterfuge.Remake.Core.Components;
using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.GameEvents.Validators;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.NaturalGameEvents.combat
{
    /// <summary>
    /// Performs specialist combat
    /// </summary>
    public class SpecialistCombat : IReversible
    {
        private readonly Entity _combatant1;
        private readonly Entity _combatant2;
        private bool _eventSuccess;

        private List<Specialist> _combatant1Specialists = new List<Specialist>();
        private List<Specialist> _combatant2Specialists = new List<Specialist>();

        /// <summary>
        /// Constructor for a specialist combat
        /// </summary>
        /// <param name="combatant1">Combatant 1</param>
        /// <param name="combatant2">Combatant 2</param>
        public SpecialistCombat(Entity combatant1, Entity combatant2)
        {
            this._combatant1 = combatant1;
            this._combatant2 = combatant2;

        }

        public bool ForwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            this._combatant1Specialists = _combatant1.GetComponent<SpecialistManager>().GetSpecialists();
            this._combatant2Specialists = _combatant1.GetComponent<SpecialistManager>().GetSpecialists();

            List<Specialist> specialists = new List<Specialist>();
            specialists.AddRange(_combatant1.GetComponent<SpecialistManager>().GetSpecialists());
            specialists.AddRange(_combatant2.GetComponent<SpecialistManager>().GetSpecialists());

            while (specialists.Count > 0)
            {
                Specialist topPriority = null;
                foreach (Specialist s in specialists)
                {
                    // If any of the specialists are invalid, cancel the event.
                    if (!Validator.ValidateSpecialist(s))
                    {
                        _eventSuccess = false;
                        return false;
                    }
                    if (topPriority == null || s.GetPriority() < topPriority.GetPriority())
                    {
                        topPriority = s;
                    }
                }
                // Apply the specialist effect to the enemey.
                Entity enemy = topPriority != null && _combatant1.GetComponent<DrillerCarrier>().GetOwner() == topPriority.GetOwner() ? _combatant2 : _combatant1;
                Entity friendly = topPriority != null && _combatant1.GetComponent<DrillerCarrier>().GetOwner() == topPriority.GetOwner() ? _combatant1 : _combatant2;
                if (topPriority != null) topPriority.ApplyEffect(state, friendly, enemy);
            }
            return true;
        }

        public bool BackwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            if (!_eventSuccess)
            {
                return false;
            }

            List<Specialist> specialists = new List<Specialist>();
            specialists.AddRange(_combatant1Specialists);
            specialists.AddRange(_combatant2Specialists);

            while (specialists.Count > 0)
            {
                Specialist lowPriority = null;
                foreach (Specialist s in specialists)
                {
                    if (lowPriority == null || s.GetPriority() >= lowPriority.GetPriority())
                    {
                        lowPriority = s;
                    }
                }
                // Apply the specialist effect to the enemey.
                Entity enemy = lowPriority != null && _combatant1.GetComponent<DrillerCarrier>().GetOwner() == lowPriority.GetOwner() ? _combatant2 : _combatant1;
                Entity friendly = lowPriority != null && _combatant1.GetComponent<DrillerCarrier>().GetOwner() == lowPriority.GetOwner() ? _combatant1 : _combatant2;
                if (lowPriority != null) lowPriority.UndoEffect(state, friendly, enemy);
            }
            return true;
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
