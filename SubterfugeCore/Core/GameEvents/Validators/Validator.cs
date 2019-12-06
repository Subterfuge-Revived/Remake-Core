using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.GameEvents.Validators
{
    public class Validator
    {
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

        public static bool validateSub(Sub sub)
        {
            if (sub == null)
                return false;
            if (!GameServer.timeMachine.getState().subExists(sub))
                return false;
            if (sub.getDrillerCount() < 0)
                return false;
            if (sub.getSpecialistManager() == null)
                return false;
            if (sub.getSpecialistManager().getSpecialistCount() < 0)
                return false;
            if (sub.getSpecialistManager().getSpecialistCount() > sub.getSpecialistManager().getCapacity())
                return false;
            if (!GameServer.timeMachine.getState().playerExists(sub.getOwner()))
                return false;
            return true;
        }

        public static bool validateOutpost(Outpost outpost)
        {
            if (outpost == null)
                return false;
            if (!GameServer.timeMachine.getState().outpostExists(outpost))
                return false;
            if (outpost.getDrillerCount() < 0)
                return false;
            if (outpost.getSpecialistManager() == null)
                return false;
            if (outpost.getSpecialistManager().getSpecialistCount() < 0)
                return false;
            if (outpost.getSpecialistManager().getSpecialistCount() > outpost.getSpecialistManager().getCapacity())
                return false;
            if (outpost.getOwner() != null && !GameServer.timeMachine.getState().playerExists(outpost.getOwner()))
                return false;
            return true;
        }

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
