using System.Collections.Generic;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Helmsman : Specialist
    {
        private List<float> speedPerLevel = new List<float>() { 0.5f, 1.0f, 1.5f };
        
        public Helmsman(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAtLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpeedManager>().IncreaseSpeed(speedPerLevel[_level]);
            }
        }

        public override void LeaveLocation(IEntity entity, TimeMachine timeMachine)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<SpeedManager>().DecreaseSpeed(speedPerLevel[_level]);
            }
        }

        public override void OnCapture(bool isCaptured, IEntity entity, TimeMachine timeMachine)
        {
            entity.GetComponent<SpeedManager>().DecreaseSpeed(speedPerLevel[_level]);
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Helmsman;
        }

        public override string GetDescription()
        {
            return $"The Helmsman travels {speedPerLevel} units faster.";
        }
    }
}