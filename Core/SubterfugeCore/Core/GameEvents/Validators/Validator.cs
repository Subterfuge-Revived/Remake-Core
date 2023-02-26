using Subterfuge.Remake.Core.Entities;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.Entities.Specialists;

namespace Subterfuge.Remake.Core.GameEvents.Validators
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
        /// <param name="state">The gamestate</param>
        /// <param name="combatable">The ICombatable to validate</param>
        /// <returns></returns>
        public static bool ValidateICombatable(GameState state, IEntity combatable)
        {
            if(combatable is Outpost)
            {
                return ValidateOutpost(state, (Outpost)combatable);
            } else if (combatable is Sub)
            {
                return ValidateSub(state, (Sub)combatable);
            }
            return false;
        }

        /// <summary>
        /// Validates a sub
        /// </summary>
        /// <param name="state">The game state</param>
        /// <param name="sub">The sub to validate</param>
        /// <returns>If the sub is valid</returns>
        public static bool ValidateSub(GameState state, Sub sub)
        {
            if (sub == null)
                return false;
            if (!state.SubExists(sub))
                return false;
            if (sub.GetComponent<DrillerCarrier>().GetDrillerCount() < 0)
                return false;
            if (sub.GetComponent<SpecialistManager>() == null)
                return false;
            if (sub.GetComponent<SpecialistManager>().GetSpecialistCount() < 0)
                return false;
            if (sub.GetComponent<SpecialistManager>().GetSpecialistCount() > sub.GetComponent<SpecialistManager>().GetCapacity())
                return false;
            if (sub.GetComponent<DrillerCarrier>().GetOwner() != null && !state.PlayerExists(sub.GetComponent<DrillerCarrier>().GetOwner()))
                return false;
            return true;
        }

        /// <summary>
        /// Validates an outpost
        /// </summary>
        /// <param name="state">The game state</param>
        /// <param name="outpost">The outpost to validate</param>
        /// <returns>If the outpost is valid</returns>
        public static bool ValidateOutpost(GameState state, Outpost outpost)
        {
            if (outpost == null)
                return false;
            if (!state.OutpostExists(outpost))
                return false;
            if (outpost.GetComponent<DrillerCarrier>().GetDrillerCount() < 0)
                return false;
            if (outpost.GetComponent<SpecialistManager>() == null)
                return false;
            if (outpost.GetComponent<SpecialistManager>().GetSpecialistCount() < 0)
                return false;
            if (outpost.GetComponent<SpecialistManager>().GetSpecialistCount() > outpost.GetComponent<SpecialistManager>().GetCapacity())
                return false;
            if (outpost.GetComponent<DrillerCarrier>().GetOwner() != null && !state.PlayerExists(outpost.GetComponent<DrillerCarrier>().GetOwner()))
                return false;
            return true;
        }

        
        /// <summary>
        /// Validates a specialist
        /// </summary>
        /// <param name="specialist">The specialist to validate</param>
        /// <returns>If the specialist is valid</returns>
        public static bool ValidateSpecialist(Specialist specialist)
        {
            if (specialist.GetOwner() == null)
                return false;
            return true;
        }

    }
}
