using SubterfugeCore.Core.Entities.Specialists;
using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.GameEvents.Validators
{
    /// <summary>
    /// Validation object to ensure that objects exists and are properly created.
    /// To prevent hacks
    /// </summary>
    public class Validator
    {
        /// <summary>
        /// Validates an ICombatable
        /// </summary>
        /// <param name="combatable">The ICombatable to validate</param>
        /// <returns></returns>
        public static bool validateICombatable(ICombatable combatable)
        {
            if(combatable is Outpost)
            {
                return validateOutpost((Outpost)combatable);
            } else if (combatable is Sub)
            {
                return validateSub((Sub)combatable);
            }
            return false;
        }
        
        /// <summary>
        /// Validates a sub
        /// </summary>
        /// <param name="sub">The sub to validate</param>
        /// <returns>If the sub is valid</returns>
        public static bool validateSub(Sub sub)
        {
            if (sub == null)
                return false;
            if (!Game.timeMachine.getState().subExists(sub))
                return false;
            if (sub.getDrillerCount() < 0)
                return false;
            if (sub.getSpecialistManager() == null)
                return false;
            if (sub.getSpecialistManager().getSpecialistCount() < 0)
                return false;
            if (sub.getSpecialistManager().getSpecialistCount() > sub.getSpecialistManager().getCapacity())
                return false;
            if (!Game.timeMachine.getState().playerExists(sub.getOwner()))
                return false;
            return true;
        }

        /// <summary>
        /// Validates an outpost
        /// </summary>
        /// <param name="outpost">The outpost to validate</param>
        /// <returns>If the outpost is valid</returns>
        public static bool validateOutpost(Outpost outpost)
        {
            if (outpost == null)
                return false;
            if (!Game.timeMachine.getState().outpostExists(outpost))
                return false;
            if (outpost.getDrillerCount() < 0)
                return false;
            if (outpost.getSpecialistManager() == null)
                return false;
            if (outpost.getSpecialistManager().getSpecialistCount() < 0)
                return false;
            if (outpost.getSpecialistManager().getSpecialistCount() > outpost.getSpecialistManager().getCapacity())
                return false;
            if (outpost.getOwner() != null && !Game.timeMachine.getState().playerExists(outpost.getOwner()))
                return false;
            return true;
        }

        
        /// <summary>
        /// Validates a specialist
        /// </summary>
        /// <param name="specialist">The specialist to validate</param>
        /// <returns>If the specialist is valid</returns>
        public static bool validateSpecialist(Specialist specialist)
        {
            if (specialist.getOwner() == null)
                return false;
            if (specialist.getPriority() <= 0)
                return false;
            return true;
        }

    }
}
