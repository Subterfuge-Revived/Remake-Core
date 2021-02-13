using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.GameEvents.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.ReversibleEvents
{
    /// <summary>
    /// Performs specialist combat
    /// </summary>
    public class SpecialistCombat : IReversible
    {
        ICombatable _combatant1;
        ICombatable _combatant2;
        bool _eventSuccess = false;

        List<Specialist> _combatant1Specialists = new List<Specialist>();
        List<Specialist> _combatant2Specialists = new List<Specialist>();

        /// <summary>
        /// Constructor for a specialist combat
        /// </summary>
        /// <param name="combatant1">Combatant 1</param>
        /// <param name="combatant2">Combatant 2</param>
        public SpecialistCombat(ICombatable combatant1, ICombatable combatant2)
        {
            this._combatant1 = combatant1;
            this._combatant2 = combatant2;

        }

        public bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            this._combatant1Specialists = _combatant1.GetSpecialistManager().GetSpecialists();
            this._combatant2Specialists = _combatant1.GetSpecialistManager().GetSpecialists();

            List<Specialist> specialists = new List<Specialist>();
            specialists.AddRange(_combatant1.GetSpecialistManager().GetSpecialists());
            specialists.AddRange(_combatant2.GetSpecialistManager().GetSpecialists());

            while (specialists.Count > 0)
            {
                Specialist topPriority = null;
                foreach (Specialist s in specialists)
                {
                    // If any of the specialists are invalid, cancel the event.
                    if (!Validator.ValidateSpecialist(s))
                    {
                        this._eventSuccess = false;
                        return false;
                    }
                    if (topPriority == null || s.GetPriority() < topPriority.GetPriority())
                    {
                        topPriority = s;
                    }
                }
                // Apply the specialist effect to the enemey.
                ICombatable enemy = _combatant1.GetOwner() == topPriority.GetOwner() ? _combatant2 : _combatant1;
                ICombatable friendly = _combatant1.GetOwner() == topPriority.GetOwner() ? _combatant1 : _combatant2;
                topPriority.ApplyEffect(friendly, enemy);
            }
            return true;
        }

        public bool BackwardAction(TimeMachine timeMachine, GameState state)
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
                ICombatable enemy = _combatant1.GetOwner() == lowPriority.GetOwner() ? _combatant2 : _combatant1;
                ICombatable friendly = _combatant1.GetOwner() == lowPriority.GetOwner() ? _combatant1 : _combatant2;
                lowPriority.UndoEffect(friendly, enemy);
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
