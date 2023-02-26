using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Entities.Positions;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

namespace Subterfuge.Remake.Core.Entities.Specialists.Specialists
{
    public class Scrutineer : Specialist
    {
        public Scrutineer(Player owner) : base(owner, false)
        {
        }

        public override void ArriveAt(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<ShieldRegenerationComponent>().ShieldRegenerator.ChangeTicksPerProductionCycle(-1 * Constants.BASE_SHIELD_REGENERATION_TICKS / 2);
                entity.GetComponent<PositionManager>().OnPostCombat += OnPostCombat;
            }
        }

        public override void LeaveLocation(IEntity entity)
        {
            if (!_isCaptured)
            {
                entity.GetComponent<ShieldRegenerationComponent>().ShieldRegenerator.ChangeTicksPerProductionCycle( Constants.BASE_SHIELD_REGENERATION_TICKS / 2);
                entity.GetComponent<PositionManager>().OnPostCombat -= OnPostCombat;
            }
        }

        public override void OnCaptured(IEntity captureLocation)
        {
            captureLocation.GetComponent<ShieldRegenerationComponent>().ShieldRegenerator.ChangeTicksPerProductionCycle( Constants.BASE_SHIELD_REGENERATION_TICKS / 2);
            captureLocation.GetComponent<PositionManager>().OnPostCombat -= OnPostCombat;
        }

        public override SpecialistTypeId GetSpecialistId()
        {
            return SpecialistTypeId.Scrutineer;
        }
        
        private void OnPostCombat(object positionManager, PostCombatEventArgs postCombatEventArgs)
        {
            if (postCombatEventArgs.Direction == TimeMachineDirection.FORWARD)
            {
                if (GetLevel() >= 3)
                {
                    var winningEntity = postCombatEventArgs.CombatResolution.Winner;
                    if (Equals(winningEntity.GetComponent<DrillerCarrier>().GetOwner(), _owner) && winningEntity is Outpost)
                    {
                        winningEntity.GetComponent<ShieldManager>().SetShields(winningEntity.GetComponent<ShieldManager>().GetShieldCapacity());
                    }
                }
            }
            else
            {
                if (GetLevel() >= 3)
                {
                    var winningEntity = postCombatEventArgs.CombatResolution.Winner;
                    if (Equals(winningEntity.GetComponent<DrillerCarrier>().GetOwner(), _owner) && winningEntity is Outpost)
                    {
                        // TODO: This is not quite accurate yet.... Figure out a way to undo this...
                        // Need a better way to enforce some forward/backward events
                        winningEntity.GetComponent<ShieldManager>().SetShields(9999);
                    }
                }
            }
        }
    }
}