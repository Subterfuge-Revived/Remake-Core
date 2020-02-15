using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.GameEvents.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.GameEvents.ReversibleEvents
{
    /// <summary>
    /// Performs specialist combat
    /// </summary>
    public class SpecialistCombat : IReversible
    {
        ICombatable combatant1;
        ICombatable combatant2;
        bool eventSuccess = false;

        List<Specialist> combatant1Specialists = new List<Specialist>();
        List<Specialist> combatant2Specialists = new List<Specialist>();

        /// <summary>
        /// Constructor for a specialist combat
        /// </summary>
        /// <param name="combatant1">Combatant 1</param>
        /// <param name="combatant2">Combatant 2</param>
        public SpecialistCombat(ICombatable combatant1, ICombatable combatant2)
        {
            this.combatant1 = combatant1;
            this.combatant2 = combatant2;

        }
        
        /// <summary>
        /// Undoes the specialist combat
        /// </summary>
        /// <returns>If the event was undone</returns>
        public bool backwardAction()
        {
            if (!eventSuccess)
            {
                return false;
            }

            List<Specialist> specialists = new List<Specialist>();
            specialists.AddRange(combatant1Specialists);
            specialists.AddRange(combatant2Specialists);

            while (specialists.Count > 0)
            {
                Specialist lowPriority = null;
                foreach (Specialist s in specialists)
                {
                    if (lowPriority == null || s.getPriority() >= lowPriority.getPriority())
                    {
                        lowPriority = s;
                    }
                }
                // Apply the specialist effect to the enemey.
                ICombatable enemy = combatant1.getOwner() == lowPriority.getOwner() ? combatant2 : combatant1;
                ICombatable friendly = combatant1.getOwner() == lowPriority.getOwner() ? combatant1 : combatant2;
                lowPriority.undoEffect(friendly, enemy);
            }
            return true;
        }

        /// <summary>
        /// Applies the specialist combat
        /// </summary>
        /// <returns>If the event was successful</returns>
        public bool forwardAction()
        {
            this.combatant1Specialists = combatant1.getSpecialistManager().getSpecialists();
            this.combatant2Specialists = combatant1.getSpecialistManager().getSpecialists();

            List<Specialist> specialists = new List<Specialist>();
            specialists.AddRange(combatant1.getSpecialistManager().getSpecialists());
            specialists.AddRange(combatant2.getSpecialistManager().getSpecialists());

            while (specialists.Count > 0)
            {
                Specialist topPriority = null;
                foreach (Specialist s in specialists)
                {
                    // If any of the specialists are invalid, cancel the event.
                    if (!Validator.validateSpecialist(s))
                    {
                        this.eventSuccess = false;
                        return false;
                    }
                    if (topPriority == null || s.getPriority() < topPriority.getPriority())
                    {
                        topPriority = s;
                    }
                }
                // Apply the specialist effect to the enemey.
                ICombatable enemy = combatant1.getOwner() == topPriority.getOwner() ? combatant2 : combatant1;
                ICombatable friendly = combatant1.getOwner() == topPriority.getOwner() ? combatant1 : combatant2;
                topPriority.applyEffect(friendly, enemy);
            }
            return true;
        }
    }
}
